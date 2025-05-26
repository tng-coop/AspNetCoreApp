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

        [Inject] public IPublicationTreeService PublicationTreeService { get; set; } = default!;
        [Inject] public ICalendarEventService CalendarEventService { get; set; } = default!;

        private CategoryDto? category;
        private List<CategoryDto> subcats = new();
        private List<CategoryDto> siblingCats = new();
        private PublicationReadDto? primaryPost;
        private List<PublicationReadDto> otherPosts = new();
        private List<CalendarEventDto> calendarEvents = new();

        private List<CategoryTreeNode>? tree;

        private PublicationReadDto? pub;
        private CategoryDto? parentCategory;
        private string fullCategoryPath = string.Empty;
        private string fullArticlePath = string.Empty;

        protected override async Task OnParametersSetAsync()
        {
            if (Nav.Uri.TrimEnd('/')
                    .EndsWith(Nav.BaseUri.TrimEnd('/')))
            {
                CategorySlug = "home";
            }

            tree = await PublicationTreeService.GetTreeAsync();
            if (string.IsNullOrEmpty(ArticleSlug))
            {
                var node = FindNode(tree, CategorySlug);
                var first = node?.Publications.FirstOrDefault();
                if (first != null)
                {
                    ArticleSlug = first.Slug;
                }
            }

            await LoadPublicationAsync();
            await LoadCategoryAsync();

        }

        private Task LoadCategoryAsync()
        {
            var node = FindNode(tree, CategorySlug);
            category = node?.Category;
            if (category == null)
                return Task.CompletedTask;

            subcats = node.Children.Select(c => c.Category).ToList();

            var parentNode = FindParentNode(tree, CategorySlug);
            if (parentNode != null)
            {
                siblingCats = parentNode.Children.Select(c => c.Category).ToList();
            }
            else
            {
                siblingCats = tree?.Select(c => c.Category).ToList() ?? new List<CategoryDto>();
            }

            var all = node.Publications;
            primaryPost = all.FirstOrDefault();
            otherPosts = all.Skip(1).ToList();
            return Task.CompletedTask;
        }

        private async Task LoadPublicationAsync()
        {
            var node = FindNode(tree, CategorySlug);
            pub = node?.Publications.FirstOrDefault(p => p.Slug == ArticleSlug);

            if (pub != null && pub.Status != "Published")
            {
                pub = null;
                return;
            }

            if (pub?.CategoryId != null)
            {
                var ancestry = await CategoryService.GetAncestryAsync(pub.CategoryId.Value);
                parentCategory = ancestry.LastOrDefault();
                var crumbs = ancestry
                              .Select(CategoryUtils.LocalizedName)
                              .Append(pub.CategoryName!)
                              .ToList();
                var crumbs2 = new List<string>(crumbs)
                              {
                                  PublicationUtils.LocalizedTitle(pub)
                              };

                fullCategoryPath = string.Join(" > ", crumbs);
                fullArticlePath = string.Join(" > ", crumbs2);
            }

            if (pub != null)
            {
                calendarEvents = await CalendarEventService.ListByPublicationAsync(pub.Id);
            }
        }

        private async Task SetAsDraft()
        {
            if (pub == null) return;

            await PublicationService.UnpublishAsync(pub.Id);
            pub = await PublicationService.GetAsync(pub.Id);
            StateHasChanged();
        }

        private static CategoryTreeNode? FindNode(IEnumerable<CategoryTreeNode>? nodes, string slug)
        {
            if (nodes == null) return null;
            foreach (var n in nodes)
            {
                if (n.Category.Slug == slug)
                    return n;
                var child = FindNode(n.Children, slug);
                if (child != null)
                    return child;
            }
            return null;
        }

        private static CategoryTreeNode? FindParentNode(IEnumerable<CategoryTreeNode>? nodes, string slug)
        {
            if (nodes == null) return null;
            foreach (var n in nodes)
            {
                if (n.Children.Any(c => c.Category.Slug == slug))
                    return n;

                var child = FindParentNode(n.Children, slug);
                if (child != null)
                    return child;
            }
            return null;
        }
    }
}
