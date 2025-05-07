using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BlazorWebApp.Data;
using BlazorWebApp.Models;

namespace BlazorWebApp.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _db;
        public CategoryService(ApplicationDbContext db) => _db = db;

        public async Task<List<CategoryDto>> ListAsync()
        {
            return await _db.Categories
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDto {
                    Id = c.Id,
                    Name = c.Name,
                    ParentCategoryId = c.ParentCategoryId
                })
                .ToListAsync();
        }
    }
}
