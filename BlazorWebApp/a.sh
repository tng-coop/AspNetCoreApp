#!/usr/bin/env bash
set -e

# Create directories
mkdir -p Data Models Services Controllers wwwroot/js Components/Pages

# 1) EF Entity
cat > Data/Publication.cs << 'EOF'
using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorWebApp.Data
{
    public enum PublicationStatus { Draft, Published, Scheduled }

    public class Publication
    {
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string DeltaJson { get; set; } = string.Empty;

        public string Html { get; set; } = string.Empty;
        public PublicationStatus Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? PublishedAt { get; set; }
    }
}
EOF

# 2) DbContext addition
cat > Data/ApplicationDbContext.cs << 'EOF'
using Microsoft.EntityFrameworkCore;

namespace BlazorWebApp.Data
{
    public partial class ApplicationDbContext
    {
        public DbSet<Publication> Publications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Publication>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.CreatedAt)
                      .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
                entity.Property(p => p.Status)
                      .HasDefaultValue(PublicationStatus.Draft);
                entity.HasIndex(p => p.CreatedAt);
            });
        }
    }
}
EOF

# 3) DTOs
cat > Models/PublicationWriteDto.cs << 'EOF'
using System.ComponentModel.DataAnnotations;

namespace BlazorWebApp.Models
{
    public class PublicationWriteDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string DeltaJson { get; set; } = string.Empty;
    }
}
EOF

cat > Models/PublicationReadDto.cs << 'EOF'
using System;

namespace BlazorWebApp.Models
{
    public class PublicationReadDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Html { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? PublishedAt { get; set; }
    }
}
EOF

# 4) Service interface & implementation
cat > Services/IPublicationService.cs << 'EOF'
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorWebApp.Models;

namespace BlazorWebApp.Services
{
    public interface IPublicationService
    {
        Task<PublicationReadDto> CreateAsync(PublicationWriteDto dto);
        Task<List<PublicationReadDto>> ListAsync();
        Task<PublicationReadDto?> GetAsync(Guid id);
        Task PublishAsync(Guid id);
    }
}
EOF

cat > Services/PublicationService.cs << 'EOF'
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BlazorWebApp.Data;
using BlazorWebApp.Models;

namespace BlazorWebApp.Services
{
    public class PublicationService : IPublicationService
    {
        private readonly ApplicationDbContext _db;
        public PublicationService(ApplicationDbContext db) => _db = db;

        public async Task<PublicationReadDto> CreateAsync(PublicationWriteDto dto)
        {
            var pub = new Publication
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                DeltaJson = dto.DeltaJson,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _db.Publications.Add(pub);
            await _db.SaveChangesAsync();
            return ToDto(pub);
        }

        public async Task<List<PublicationReadDto>> ListAsync()
        {
            return await _db.Publications
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => ToDto(p))
                .ToListAsync();
        }

        public async Task<PublicationReadDto?> GetAsync(Guid id)
        {
            var p = await _db.Publications.FindAsync(id);
            return p is null ? null : ToDto(p);
        }

        public async Task PublishAsync(Guid id)
        {
            var p = await _db.Publications.FindAsync(id);
            if (p == null) throw new KeyNotFoundException();
            p.Status = PublicationStatus.Published;
            p.PublishedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();
        }

        private static PublicationReadDto ToDto(Publication p) => new()
        {
            Id = p.Id,
            Title = p.Title,
            Html = p.Html,
            Status = p.Status.ToString(),
            CreatedAt = p.CreatedAt,
            PublishedAt = p.PublishedAt
        };
    }
}
EOF

# 5) Controller
cat > Controllers/PublicationsController.cs << 'EOF'
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BlazorWebApp.Models;
using BlazorWebApp.Services;

namespace BlazorWebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PublicationsController : ControllerBase
    {
        private readonly IPublicationService _svc;
        public PublicationsController(IPublicationService svc) => _svc = svc;

        [HttpGet]
        public Task<List<PublicationReadDto>> List() => _svc.ListAsync();

        [HttpGet("{id:guid}")]
        public Task<PublicationReadDto?> Get(Guid id) => _svc.GetAsync(id);

        [HttpPost]
        public Task<PublicationReadDto> Create([FromBody] PublicationWriteDto dto) =>
            _svc.CreateAsync(dto);

        [HttpPost("{id:guid}/publish")]
        public Task Publish(Guid id) => _svc.PublishAsync(id);
    }
}
EOF

# 6) JS interop stub
cat > wwwroot/js/quillInterop.js << 'EOF'
// quillInterop.js
export async function initializeQuill(selector) {
  const load = src => new Promise((r, e) => {
    const s = document.createElement('script');
    s.src = src; s.onload = r; s.onerror = e;
    document.head.appendChild(s);
  });
  await load('https://cdn.jsdelivr.net/npm/quill@2.0.3/dist/quill.min.js');
  Quill.register({'modules/table-better': QuillTableBetter}, true);
  window.quill = new Quill(selector, {
    theme: 'snow',
    modules: {
      table: false,
      'table-better': {},
      keyboard: { bindings: QuillTableBetter.keyboardBindings },
      toolbar: {
        container: [
          [{ header: [1,2,3,false] }],
          ['bold','italic','underline','strike'],
          ['blockquote','code-block'],
          [{ list:'ordered' }, { list:'bullet' }],
          ['link','image','video'],
          ['table-better'],
          ['clean']
        ]
      }
    },
    placeholder: 'Type here…'
  });
}

export function getDeltaJson() {
  return JSON.stringify(window.quill.getContents());
}

export function getHtml() {
  return window.quill.root.innerHTML;
}
EOF

# 7) Blazor Pages
cat > Components/Pages/Editor.razor << 'EOF'
@page "/editor/{Id:guid?}"
@inject IJSRuntime JS
@inject HttpClient Http

<h3>@(Id == null ? "New Post" : "Edit Post")</h3>
<input class="form-control mb-2" placeholder="Title" @bind="dto.Title" />

<div id="editor" style="height:300px; background:#fff;"></div>

<button class="btn btn-primary" @onclick="Save">Save Draft</button>
<button class="btn btn-success ms-2" @onclick="Publish" disabled="@(!canPublish)">Publish</button>

@code {
  [Parameter] public Guid? Id { get; set; }
  private PublicationWriteDto dto = new();
  private IJSObjectReference? mod;
  private bool canPublish => !string.IsNullOrWhiteSpace(dto.Title) && !string.IsNullOrWhiteSpace(dto.DeltaJson);

  protected override async Task OnAfterRenderAsync(bool first) {
    if (first) {
      mod = await JS.InvokeAsync<IJSObjectReference>("import", "./js/quillInterop.js");
      await mod.InvokeVoidAsync("initializeQuill", "#editor");
      if (Id.HasValue) {
        var existing = await Http.GetFromJsonAsync<PublicationReadDto>($"api/publications/{Id}");
        dto.Title = existing?.Title ?? "";
        await mod.InvokeVoidAsync("setContents", existing?.DeltaJson);
      }
    }
  }

  private async Task Save() {
    dto.DeltaJson = await mod!.InvokeAsync<string>("getDeltaJson");
    dto = await Http.PostAsJsonAsync("api/publications", dto)
                    .Result.Content.ReadFromJsonAsync<PublicationWriteDto>() ?? dto;
  }

  private async Task Publish() {
    await Save();
    await Http.PostAsync($"api/publications/{dto.Id}/publish", null);
  }
}
EOF

cat > Components/Pages/PublicationsList.razor << 'EOF'
@page "/publications"
@inject HttpClient Http

<h3>All Posts</h3>
<ul>
@foreach(var p in pubs) {
  <li>
    <a href="/publications/@p.Id">@p.Title</a>
    (@p.Status)
  </li>
}
</ul>

@code {
  private List<PublicationReadDto> pubs = new();
  protected override async Task OnInitializedAsync() {
    pubs = await Http.GetFromJsonAsync<List<PublicationReadDto>>("api/publications") ?? pubs;
  }
}
EOF

cat > Components/Pages/PublicationDetails.razor << 'EOF'
@page "/publications/{Id:guid}"
@inject HttpClient Http

@code {
  [Parameter] public Guid Id { get; set; }
  private PublicationReadDto? pub;
  protected override async Task OnInitializedAsync() {
    pub = await Http.GetFromJsonAsync<PublicationReadDto>($"api/publications/{Id}");
  }
}
@if (pub == null) {
  <p><em>Not found.</em></p>
} else {
  <h1>@pub.Title</h1>
  <article>@(new MarkupString(pub.Html))</article>
  <p><small>@pub.CreatedAt</small></p>
}
EOF

# 8) Program.cs updates (append registrations if not present)
grep -q "AddScoped<IPublicationService" Program.cs \
  || sed -i "/AddScoped<INoteService/a \ \ \ \ builder.Services.AddScoped<IPublicationService, PublicationService>();" Program.cs

grep -q "MapControllers" Program.cs \
  || sed -i "/app.MapRazorComponents<App()/i \ \ \ \ app.MapControllers(); // publication API" Program.cs

echo "Scaffold complete."
