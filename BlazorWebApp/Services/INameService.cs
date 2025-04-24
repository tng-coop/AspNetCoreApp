using System;
using System.Threading.Tasks;

namespace BlazorWebApp.Services
{
    public interface INameService
    {
        Task<String?> GetLatestForNameAsync(string name, string? ownerId = null);
    }
}
