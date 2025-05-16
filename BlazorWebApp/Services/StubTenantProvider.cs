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
            var tenant = db.Tenants.AsNoTracking().FirstOrDefault();
            if (tenant is null)
            {
                throw new InvalidOperationException("No tenants found in the database. Please seed at least one tenant before running the application.");
            }

            Current = tenant;
        }
    }
}
