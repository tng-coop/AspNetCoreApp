using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TinyMCE.Blazor;
using BlazorWebApp.Models;
using BlazorWebApp.Services;

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

  // Can publish only if title and content are non-empty
  private bool canPublish =>
    !string.IsNullOrWhiteSpace(dto.Title) &&
    !string.IsNullOrWhiteSpace(dto.Html);

  private static bool ContainsOnlyAscii(string text) => text.All(c => c <= sbyte.MaxValue);
  private bool slugAscii => ContainsOnlyAscii(dto.Slug);

  // TinyMCE configuration dictionary—with YouTube paste/embed support
private readonly Dictionary<string, object> editorConfig = new()
{
  ["height"] = 300,
  ["menubar"] = "file edit view insert format tools media table help",
  ["plugins"] = new[] { "link", "lists", "code", "image", "paste", "table", "media" },
  ["toolbar"] = "undo redo | bold italic underline | alignleft aligncenter | bullist numlist | link image media | code",
  ["images_upload_url"] = "/api/images/upload",
  ["automatic_uploads"] = true,
  ["paste_data_images"] = true,
  ["promotion"] = false,
  ["branding"] = false,

  // allow iframes and your custom element
  ["custom_elements"] = "my-component",
  ["extended_valid_elements"] = "iframe[src|width|height|frameborder|allowfullscreen],my-component[*]",
  // ["skin"] = "oxide-dark",
  // ["content_css"] = "dark",
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
        dto.Slug = existing.Slug;
        dto.Html = existing.Html;
        dto.FeaturedOrder = existing.FeaturedOrder;
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
    Nav.NavigateTo($"/_cms/editor/{Id}", replace: true);
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
    dto.Html = updated.Html;
    dto.CategoryId = updated.CategoryId;
    dto.FeaturedOrder = updated.FeaturedOrder;

    // reload history
    if (Id is Guid pubId)
      revisions = await PublicationService.ListRevisionsAsync(pubId);
  }
}
}