using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using BlazorWebApp.Data;
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
  [Parameter] public string? Tenant { get; set; }

  private PublicationWriteDto dto = new();
  private List<CategoryDto> categories = new();
  private List<(Guid Id, string DisplayName)> categoryOptions = new();
  private EditContext editContext = default!;
  private bool loadedExisting;
  private List<RevisionDto>? revisions;
  private bool uploadInProgress;
  private List<CalendarEventRow>? eventRows;

  private class CalendarEventRow
  {
    public Guid? Id { get; set; }
    public CalendarEventWriteDto Dto { get; set; } = new();
  }

  // Can publish only if title and content are non-empty
  private bool canPublish =>
    !string.IsNullOrWhiteSpace(dto.Title) &&
    ((dto.Mode == PublicationContentMode.Html && !string.IsNullOrWhiteSpace(dto.Html)) ||
     (dto.Mode == PublicationContentMode.Pdf && dto.PdfFileId != null));

  private static bool ContainsOnlyAscii(string text) => text.All(c => c <= sbyte.MaxValue);
  private bool slugAscii => ContainsOnlyAscii(dto.Slug);

  // TinyMCE configuration dictionary—with YouTube paste/embed support
  private readonly Dictionary<string, object> editorConfig = new()
  {
  ["height"] = 300,
  ["menubar"] = "file edit view insert format tools media table help",
  ["plugins"] = new[] { "link", "lists", "code", "image", "paste", "table", "media" },
  ["toolbar"] = "undo redo | bold italic underline | alignleft aligncenter | bullist numlist | link image media | code",
  ["images_upload_url"] = "/api/files/upload",
  ["automatic_uploads"] = true,
  ["paste_data_images"] = true,
    ["promotion"] = false,
    ["branding"] = false,
    ["license_key"] = "gpl",



  // allow iframes and your custom element
    ["custom_elements"] = "my-component",
    ["extended_valid_elements"] = "iframe[src|width|height|frameborder|allowfullscreen],my-component[*]",
    // ["skin"] = "oxide-dark",
    // ["content_css"] = "dark",
  };

  private string editorKey = Guid.NewGuid().ToString();

  private void SetEditorLanguage()
  {
    var lang = Localization.CurrentCulture.TwoLetterISOLanguageName;
    if (lang == "ja")
    {
      editorConfig["language"] = "ja";
      editorConfig["language_url"] = "/lib/tinymce/langs/langs7/ja.js";
    }
    else
    {
      editorConfig.Remove("language");
      editorConfig.Remove("language_url");
    }

    editorKey = Guid.NewGuid().ToString();
  }

  protected override async Task OnInitializedAsync()
  {
    editContext = new EditContext(dto);
    SetEditorLanguage();
    categories = await CategoryService.ListAsync();
    categoryOptions.Clear();

    foreach (var cat in categories)
    {
      var ancestors = await CategoryService.GetAncestryAsync(cat.Id);
      var fullPath = string.Join(" > ",
                        ancestors.Select(CategoryUtils.LocalizedName)
                                 .Append(CategoryUtils.LocalizedName(cat)));
      categoryOptions.Add((cat.Id, fullPath));
    }
    categoryOptions.Sort((a, b) =>
      string.Compare(a.DisplayName, b.DisplayName, StringComparison.CurrentCulture));

    // Preselect the "Home" category for new posts
    var home = categories.FirstOrDefault(c => c.Slug == "home");
    if (!Id.HasValue && dto.CategoryId == null && home != null)
      dto.CategoryId = home.Id;

    if (Id.HasValue)
    {
      var existing = await PublicationService.GetAsync(Id.Value);
      if (existing != null)
      {
        loadedExisting = true;
        dto.CategoryId = existing.CategoryId;
        dto.Title = existing.Title;
        dto.TitleJa = existing.TitleJa;
        dto.Slug = existing.Slug;
        dto.Html = existing.Html;
        dto.FeaturedOrder = existing.FeaturedOrder;
        dto.Mode = existing.Mode;
        dto.PdfFileId = existing.PdfFileId;
        // load revisions
        revisions = await PublicationService.ListRevisionsAsync(Id.Value);
        // load calendar events
        eventRows = (await CalendarEventService.ListForPublicationAsync(Id.Value))
            .Select(e => new CalendarEventRow
            {
                Id = e.Id,
                Dto = new CalendarEventWriteDto
                {
                    Start = e.Start,
                    End = e.End,
                    AllDay = e.AllDay,
                    Url = e.Url
                }
            }).ToList();
      }
    }
  }

  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    await base.OnAfterRenderAsync(firstRender);
    if (firstRender)
    {
      Localization.OnChange += CultureChanged;
    }
  }

  private void CultureChanged()
  {
    InvokeAsync(() =>
    {
      SetEditorLanguage();
      StateHasChanged();
    });
  }

  public new void Dispose()
  {
    Localization.OnChange -= CultureChanged;
    base.Dispose();
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
    Nav.NavigateTo($"/_cms/_editor/{Id}", replace: true);
  }

  // AFTER either branch, refresh the history panel
  if (Id is Guid pubId)
    revisions = await PublicationService.ListRevisionsAsync(pubId);

  StateHasChanged();
}

  private void TitleChanged(ChangeEventArgs e)
  {
    dto.Title = e.Value?.ToString() ?? string.Empty;
  }

  private async Task UploadPdf(InputFileChangeEventArgs e)
  {
    if (e.FileCount == 0) return;
    var file = e.File;
    Console.WriteLine($"UploadPdf: starting upload of {file.Name} ({file.Size} bytes)");
    uploadInProgress = true;
    StateHasChanged();

    using var content = new MultipartFormDataContent();
    var streamContent = new StreamContent(file.OpenReadStream(long.MaxValue));
    content.Add(streamContent, "file", file.Name);
    var uploadUrl = Nav.ToAbsoluteUri("/api/files/upload");
    Console.WriteLine($"UploadPdf: posting to {uploadUrl}");
    var resp = await Http.PostAsync(uploadUrl, content);
    if (resp.IsSuccessStatusCode)
    {
        var result = await resp.Content.ReadFromJsonAsync<UploadResult>();
        if (result != null && Uri.TryCreate(result.location, UriKind.Absolute, out var uri))
        {
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length >= 4 && Guid.TryParse(segments[2], out var id))
                dto.PdfFileId = id;
        }
    }

    Console.WriteLine($"UploadPdf: finished with status {resp.StatusCode}");

    uploadInProgress = false;
    StateHasChanged();
  }

  private class UploadResult
  {
      public string location { get; set; } = string.Empty;
  }


  private async Task PublishAsync()
  {
    await HandleSubmit();
    if (loadedExisting && Id.HasValue)
    {
      await PublicationService.PublishAsync(Id.Value);
      var pub = await PublicationService.GetAsync(Id.Value);
      string catSlug = string.Empty;
      if (pub?.CategoryId is Guid catId)
      {
        var categories = await CategoryService.ListAsync();
        catSlug = categories.FirstOrDefault(c => c.Id == catId)?.Slug ?? string.Empty;
      }
      var target = $"/_cms/{catSlug}/{pub?.Slug}".Replace("//", "/");
      Nav.NavigateTo(target);
    }
  }

  private async Task Restore(Guid revisionId)
  {
    var updated = await PublicationService.RestoreRevisionAsync(revisionId);
    // update the editor DTO
    dto.Title = updated.Title;
    dto.TitleJa = updated.TitleJa;
    dto.Html = updated.Html;
    dto.CategoryId = updated.CategoryId;
    dto.FeaturedOrder = updated.FeaturedOrder;

    // reload history
    if (Id is Guid pubId)
      revisions = await PublicationService.ListRevisionsAsync(pubId);
  }

  private void AddEvent()
  {
    eventRows ??= new();
    eventRows.Add(new CalendarEventRow
    {
        Dto = new CalendarEventWriteDto
        {
            Start = DateTime.Now,
            End = DateTime.Now.AddHours(1)
        }
    });
  }

  private async Task SaveEvent(CalendarEventRow row)
  {
    if (!Id.HasValue) return;

    if (row.Id.HasValue)
    {
        await CalendarEventService.UpdateAsync(row.Id.Value, row.Dto);
    }
    else
    {
        var created = await CalendarEventService.CreateAsync(Id.Value, row.Dto);
        row.Id = created.Id;
    }
  }

  private async Task DeleteEvent(CalendarEventRow row)
  {
    if (row.Id.HasValue)
        await CalendarEventService.DeleteAsync(row.Id.Value);
    eventRows?.Remove(row);
  }
}
}
