using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using BlazorWebApp.Models;
using BlazorWebApp.Services;
using BlazorWebApp.Utils;

namespace BlazorWebApp.Components.Pages
{
    public partial class Category
    {
        [Parameter] public string CategorySlug { get; set; } = string.Empty;
        [Parameter] public string? ArticleSlug { get; set; }
        [Parameter] public string? Tenant { get; set; }

        private bool displayPublication;

        private CategoryDto? category;
        private List<CategoryDto> subcats = new();
        private PublicationReadDto? primaryPost;
        private List<PublicationReadDto> otherPosts = new();

        private PublicationReadDto? pub;
        private string fullCategoryPath = string.Empty;
        private string fullArticlePath = string.Empty;

        protected override async Task OnParametersSetAsync()
        {
            if (Nav.Uri.TrimEnd('/').EndsWith(Nav.BaseUri.TrimEnd('/')))
            {
                CategorySlug = "home";
                displayPublication = true;
            }

            if (string.IsNullOrEmpty(ArticleSlug))
            {
                var all = await PublicationService.ListAsync();
                var pub = all.FirstOrDefault(p => p.CategorySlug == CategorySlug);
                if (pub != null)
                {
                    ArticleSlug = pub.Slug;
                    // displayPublication = true;              // now we want to show the article
                    Console.WriteLine($"Discovered article slug {ArticleSlug}");
                }
            }
                await LoadPublicationAsync();
                await LoadCategoryAsync();

        }

        private async Task LoadCategoryAsync()
        {
            var allCats = await CategoryService.ListAsync();
            category = allCats.FirstOrDefault(c => c.Slug == CategorySlug);
            if (category == null)
                return;

            subcats = allCats
                .Where(c => c.ParentCategoryId == category.Id)
                .ToList();

            var all = (await PublicationService.ListAsync())
                        .Where(p => p.CategoryId == category.Id
                                    && p.Status == "Published")
                        .OrderBy(p => p.FeaturedOrder == 0 ? int.MaxValue : p.FeaturedOrder)
                        .ThenByDescending(p => p.PublishedAt ?? DateTimeOffset.MinValue)
                        .ToList();

            primaryPost = all.FirstOrDefault();
            otherPosts = all.Skip(1).ToList();
        }

        private async Task LoadPublicationAsync()
        {
            pub = await PublicationService.GetBySlugAsync(CategorySlug, ArticleSlug!);

            if (pub?.CategoryId != null)
            {
                var ancestry = await CategoryService.GetAncestryAsync(pub.CategoryId.Value);
                var crumbs = ancestry
                              .Select(c => c.Name)
                              .Append(pub.CategoryName!)
                              .ToList();
                var crumbs2 = new List<string>(crumbs)
                              {
                                  pub.Title
                              };

                fullCategoryPath = string.Join(" > ", crumbs);
                fullArticlePath = string.Join(" > ", crumbs2);
            }
        }

        private async Task SetAsDraft()
        {
            if (pub == null) return;

            await PublicationService.UnpublishAsync(pub.Id);
            pub = await PublicationService.GetAsync(pub.Id);
            StateHasChanged();
        }
    }
}
