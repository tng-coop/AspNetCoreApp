using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorWebApp.Models;

namespace BlazorWebApp.Services
{
    public interface ITenantService
    {
        Task<List<TenantDto>> ListAsync();
    }
}
