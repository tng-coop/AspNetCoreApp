namespace BlazorWebApp.Services
{
    /// <summary>
    /// Options for StubTenantProvider to specify a default tenant slug.
    /// </summary>
    public class StubTenantProviderOptions
    {
        /// <summary>
        /// If set, this slug will be used as the default tenant (falls back to first tenant otherwise).
        /// </summary>
        public string DefaultTenantSlug { get; set; } = string.Empty;
    }
}
