using Microsoft.Extensions.DependencyInjection;
using BlazorWebApp.Services;

namespace BlazorWebApp.Modules.Cms
{
    /// <summary>
    /// Collects all CMS-related services into a self-contained module.
    /// </summary>
    public static class CmsModule
    {
        /// <summary>
        /// Registers CMS services and related dependencies.
        /// </summary>
        public static IServiceCollection AddCmsModule(this IServiceCollection services)
        {
            // Core CMS application services
            services.AddScoped<IPublicationService, PublicationService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ITenantService, TenantService>();
            services.AddScoped<IPublicationTreeService, PublicationTreeService>();
            services.AddScoped<ITreeMenuService, TreeMenuService>();
            services.AddScoped<ICalendarEventService, CalendarEventService>();
            services.AddScoped<ICommentService, CommentService>();

            return services;
        }
    }
}
