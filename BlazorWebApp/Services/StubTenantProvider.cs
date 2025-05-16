using System.Linq;
using BlazorWebApp.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazorWebApp.Services
{
    /// <summary>
    /// Phase0 stub: always returns the first Tenant in the database.
    /// Replace with real resolver (subdomain/path) later.
    /// </summary>
    public class StubTenantProvider : ITenantProvider
    {
        public Tenant Current { get; }
        public StubTenantProvider(ApplicationDbContext db)
        {
            // Ensure at least one tenant exists
            Current = db.Tenants.AsNoTracking().First();
        }
    }
}
