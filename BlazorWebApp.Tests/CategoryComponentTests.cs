using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorWebApp.Components.Pages;
using BlazorWebApp.Models;
using BlazorWebApp.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class CategoryComponentTests : TestContext
{
    private class StubCategoryService : ICategoryService
    {
        private readonly List<CategoryDto> _categories;
        public StubCategoryService(List<CategoryDto> categories) => _categories = categories;
        public Task<List<CategoryDto>> ListAsync() => Task.FromResult(_categories);
        public Task<List<CategoryDto>> GetAncestryAsync(Guid categoryId) => Task.FromResult(new List<CategoryDto>());
        public Task<CategoryDto> CreateAsync(CategoryWriteDto dto) => throw new NotImplementedException();
    }

    private class StubPublicationService : IPublicationService
    {
        private readonly List<PublicationReadDto> _all;
        private readonly List<PublicationReadDto> _featured;
        public StubPublicationService(List<PublicationReadDto> all, List<PublicationReadDto> featured)
        {
            _all = all;
            _featured = featured;
        }
        public Task<List<PublicationReadDto>> ListAsync() => Task.FromResult(_all);
        public Task<List<PublicationReadDto>> ListFeaturedInCategoryAsync(Guid categoryId) => Task.FromResult(_featured);
        public Task<PublicationReadDto?> GetBySlugAsync(string slug)
            => Task.FromResult(_all.FirstOrDefault(p => p.Slug == slug));
        // Unused interface members
        public Task<PublicationReadDto> CreateAsync(PublicationWriteDto dto) => throw new NotImplementedException();
        public Task<PublicationReadDto?> GetAsync(Guid id) => throw new NotImplementedException();
        public Task PublishAsync(Guid id) => throw new NotImplementedException();
        public Task UpdateAsync(Guid id, PublicationWriteDto dto) => throw new NotImplementedException();
        public Task UnpublishAsync(Guid id) => throw new NotImplementedException();
        public Task DeleteAsync(Guid id) => throw new NotImplementedException();
        public Task<List<RevisionDto>> ListRevisionsAsync(Guid publicationId) => throw new NotImplementedException();
        public Task<PublicationReadDto> RestoreRevisionAsync(Guid revisionId) => throw new NotImplementedException();
    }

    [Fact]
    public void CategoryWithOnlyFeaturedDisplaysThem()
    {
        // Arrange services
        var catId = Guid.NewGuid();
        var categories = new List<CategoryDto> { new() { Id = catId, Name = "Test", Slug = "test-cat" } };
        var featuredPost = new PublicationReadDto
        {
            Id = Guid.NewGuid(),
            Title = "Featured Post",
            Slug = "featured-post",
            PublishedAt = DateTimeOffset.Now,
            CategoryId = catId
        };
        Services.AddSingleton<ICategoryService>(new StubCategoryService(categories));
        Services.AddSingleton<IPublicationService>(new StubPublicationService(new List<PublicationReadDto> { featuredPost }, new List<PublicationReadDto> { featuredPost }));

        // Act
        var cut = RenderComponent<Category>(parameters => parameters.Add(p => p.Slug, "test-cat"));

        // Assert
        Assert.Contains("Featured Post", cut.Markup);
        Assert.DoesNotContain("No published articles in this category", cut.Markup);
    }

    [Fact]
    public void DraftPostsAreNotRendered()
    {
        // Arrange services
        var catId = Guid.NewGuid();
        var categories = new List<CategoryDto> { new() { Id = catId, Name = "Test", Slug = "test-cat" } };

        var published = new PublicationReadDto
        {
            Id = Guid.NewGuid(),
            Title = "Published Post",
            Slug = "published-post",
            PublishedAt = DateTimeOffset.Now,
            Status = "Published",
            CategoryId = catId
        };

        var draft = new PublicationReadDto
        {
            Id = Guid.NewGuid(),
            Title = "Draft Post",
            Slug = "draft-post",
            Status = "Draft",
            CategoryId = catId
        };

        Services.AddSingleton<ICategoryService>(new StubCategoryService(categories));
        Services.AddSingleton<IPublicationService>(new StubPublicationService(
            new List<PublicationReadDto> { published, draft },
            new List<PublicationReadDto>()));

        // Act
        var cut = RenderComponent<Category>(parameters => parameters.Add(p => p.Slug, "test-cat"));

        // Assert
        Assert.Contains("Published Post", cut.Markup);
        Assert.DoesNotContain("Draft Post", cut.Markup);
    }
}
