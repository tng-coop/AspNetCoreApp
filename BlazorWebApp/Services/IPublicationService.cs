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
        Task UpdateAsync(Guid id, PublicationWriteDto dto);
    }
}
