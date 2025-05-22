using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorWebApp.Models;

namespace BlazorWebApp.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> ListAsync();
        Task<List<CategoryDto>> GetAncestryAsync(Guid categoryId);
        Task<CategoryDto> CreateAsync(CategoryWriteDto dto);
    }
}
