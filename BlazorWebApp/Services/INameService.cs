using System.Threading.Tasks;

namespace BlazorWebApp.Services
{
    public interface INameService
    {
        Task<string?> GetLatestForNameAsync(string name, string? ownerId = null);
        Task SetNameAsync(string name, string value, string? ownerId = null);
    }
}
