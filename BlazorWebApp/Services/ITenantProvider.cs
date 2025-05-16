using BlazorWebApp.Data;

namespace BlazorWebApp.Services
{
    /// <summary>
    /// Resolves the current Tenant for each request.
    /// </summary>
    public interface ITenantProvider { Tenant Current { get; } }
}
