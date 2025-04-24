using System;
using System.Threading.Tasks;

namespace BlazorWebApp.Services
{
    public interface INameUuidService
    {
        Task<Guid?> GetLatestUuidForNameAsync(string name, string? ownerId = null);
    }
}
