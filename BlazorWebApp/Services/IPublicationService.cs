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
        Task<PublicationReadDto?> GetBySlugAsync(string categorySlug, string slug);
        Task PublishAsync(Guid id);
        Task UpdateAsync(Guid id, PublicationWriteDto dto);
        Task UnpublishAsync(Guid id);
        /// <summary>
        /// Deletes the given publication.
        /// </summary>
        Task DeleteAsync(Guid id);

        // Revision history
        Task<List<RevisionDto>> ListRevisionsAsync(Guid publicationId);
        Task<PublicationReadDto> RestoreRevisionAsync(Guid revisionId);
        Task<List<PublicationReadDto>> ListFeaturedInCategoryAsync(Guid categoryId);
    }
}
