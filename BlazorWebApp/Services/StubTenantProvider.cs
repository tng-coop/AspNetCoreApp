using System;
using System.Linq;
using BlazorWebApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;

namespace BlazorWebApp.Services
{
    /// <summary>
    /// Phase0 stub: always returns the first Tenant in the database.
    /// Replace with real resolver (subdomain/path) later.
    /// </summary>
    public class StubTenantProvider : ITenantProvider
    {
        public Tenant Current { get; }

        public StubTenantProvider(
            IDbContextFactory<ApplicationDbContext> factory,
            IOptions<StubTenantProviderOptions> options)
        {
            using var db = factory.CreateDbContext();

            var slug = options.Value.DefaultTenantSlug;
            Tenant? tenant = null;

            if (!string.IsNullOrWhiteSpace(slug))
            {
                tenant = db.Tenants
                             .AsNoTracking()
                             .FirstOrDefault(t => t.Slug == slug);
            }

            tenant ??= db.Tenants
                            .AsNoTracking()
                            .FirstOrDefault();

            if (tenant is null)
            {
                throw new InvalidOperationException(
                    "No tenants found in the database. Please seed at least one tenant before running the application.");
            }

            Current = tenant;
        }
    }
}
