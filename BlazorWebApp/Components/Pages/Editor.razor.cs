using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TinyMCE.Blazor;
using BlazorWebApp.Models;
using BlazorWebApp.Services;
using BlazorWebApp.Utils;

namespace BlazorWebApp.Components.Pages
{
    public partial class Editor
    {
  [Parameter] public Guid? Id { get; set; }

  private PublicationWriteDto dto = new();
  private List<CategoryDto> categories = new();
  private List<(Guid Id, string DisplayName)> categoryOptions = new();
  private EditContext editContext = default!;
  private bool loadedExisting;
  private List<RevisionDto>? revisions;

  // Can publish only if title and content are non-empty
  private bool canPublish =>
    !string.IsNullOrWhiteSpace(dto.Title) &&
    !string.IsNullOrWhiteSpace(dto.Html);

  private bool slugAscii => SlugGenerator.ContainsOnlyAscii(dto.Slug);

  // TinyMCE configuration dictionary—with YouTube paste/embed support
  private readonly Dictionary<string, object> editorConfig = new()
  {
    ["height"] = 300,
    ["menubar"] = "file edit view insert format tools media table help",
    ["plugins"] = new[] { "link", "lists", "code", "image", "paste", "table", "media" },
    ["toolbar"] =
      "undo redo | bold italic underline | alignleft aligncenter | " +
      "bullist numlist | link image media | code",
    ["table_toolbar"] =
      "tableprops tabledelete | tableinsertrowbefore tableinsertrowafter | " +
      "tableinsertcolbefore tableinsertcolafter",
    ["images_upload_url"] = "/api/images/upload",
    ["automatic_uploads"] = true,
    ["paste_data_images"] = false,
    ["promotion"] = false,
    ["branding"] = false,

    // allow iframes through sanitizer
    ["extended_valid_elements"] = "iframe[src|width|height|frameborder|allowfullscreen]",

    // convert pasted YouTube links into embed iframes
    ["media_url_resolver"] = @"
function(data, resolve) {
    var youtubeMatch = data.url.match(/(?:youtu\.be\/|youtube\.com\/.*v=)([\w-_-]+)/);
    if (youtubeMatch) {
        var id = youtubeMatch[1];
        var embedUrl = 'https://www.youtube.com/embed/' + id;
        resolve({
            html: '<iframe width=""560"" height=""315"" src=""' + embedUrl + '"" frameborder=""0"" allowfullscreen></iframe>'
        });
    } else {
        resolve({ html: '' });
    }
}
"
  };

  protected override async Task OnInitializedAsync()
  {
    editContext = new EditContext(dto);
    categories = await CategoryService.ListAsync();
    categoryOptions.Clear();

    foreach (var cat in categories)
    {
      var ancestors = await CategoryService.GetAncestryAsync(cat.Id);
      var fullPath = string.Join(" > ",
                        ancestors.Select(a => a.Name)
                                 .Append(cat.Name));
      categoryOptions.Add((cat.Id, fullPath));
    }
    categoryOptions.Sort((a, b) =>
      string.Compare(a.DisplayName, b.DisplayName, StringComparison.CurrentCulture));

    if (Id.HasValue)
    {
      var existing = await PublicationService.GetAsync(Id.Value);
      if (existing != null)
      {
        loadedExisting = true;
        dto.CategoryId = existing.CategoryId;
        dto.Title = existing.Title;
        dto.Slug = existing.Slug;
        dto.Html = existing.Html;
        // load revisions
        revisions = await PublicationService.ListRevisionsAsync(Id.Value);
      }
    }
  }

private async Task HandleSubmit()
{
  if (loadedExisting && Id.HasValue)
  {
    await PublicationService.UpdateAsync(Id.Value, dto);
  }
  else
  {
    var created = await PublicationService.CreateAsync(dto);
    Id = created.Id;
    loadedExisting = true;
    // replace the URL so OnInitializedAsync will load our new Id
    Nav.NavigateTo($"/editor/{Id}", replace: true);
  }

  // AFTER either branch, refresh the history panel
  if (Id is Guid pubId)
    revisions = await PublicationService.ListRevisionsAsync(pubId);

  StateHasChanged();
}

  private void TitleChanged(ChangeEventArgs e)
  {
    dto.Title = e.Value?.ToString() ?? string.Empty;
    if (string.IsNullOrWhiteSpace(dto.Slug))
      dto.Slug = SlugGenerator.Generate(dto.Title);
  }


  private async Task PublishAsync()
  {
    await HandleSubmit();
    if (loadedExisting && Id.HasValue)
    {
      await PublicationService.PublishAsync(Id.Value);
      Nav.NavigateTo($"/publications/{Id}");
    }
  }

  private async Task Restore(Guid revisionId)
  {
    var updated = await PublicationService.RestoreRevisionAsync(revisionId);
    // update the editor DTO
    dto.Title = updated.Title;
    dto.Html = updated.Html;
    dto.CategoryId = updated.CategoryId;

    // reload history
    if (Id is Guid pubId)
      revisions = await PublicationService.ListRevisionsAsync(pubId);
  }
}
}