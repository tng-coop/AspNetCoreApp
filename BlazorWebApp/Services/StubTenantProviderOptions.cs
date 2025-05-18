using System.ComponentModel.DataAnnotations;

namespace BlazorWebApp.Services
{
    public class StubTenantProviderOptions
    {
        [Required(ErrorMessage = "DefaultTenantSlug is required.")]
        public string DefaultTenantSlug { get; set; } = string.Empty;
    }
}
