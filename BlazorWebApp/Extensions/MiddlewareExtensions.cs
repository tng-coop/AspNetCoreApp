using Microsoft.Extensions.FileProviders;
using System.Globalization;

namespace BlazorWebApp.Extensions;

public static class MiddlewareExtensions
{
    public static WebApplication UseCustomMiddleware(this WebApplication app)
    {
        // Use Forwarded Headers and HSTS in production
        if (!app.Environment.IsDevelopment())
        {
            app.UseForwardedHeaders();
            app.UseHsts();
            app.UseHttpsRedirection();
        }
        else
        {
            var certPath = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "cert"));
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(certPath),
                RequestPath = "/cert"
            });
            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = new PhysicalFileProvider(certPath),
                RequestPath = "/cert"
            });
        }

        app.Use(async (context, next) =>
        {
            if (context.Request.Cookies.TryGetValue("blazorCulture", out var culture))
            {
                var cultureInfo = new CultureInfo(culture);
                CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
                CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
            }

            await next();
        });

        // Migrations and error handling
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
        }

        app.UseStaticFiles();
        var cachePath = Path.Combine(app.Environment.WebRootPath, "imgcache");
        Directory.CreateDirectory(cachePath);
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(cachePath),
            RequestPath = "/imgcache",
            OnPrepareResponse = ctx => ctx.Context.Response.Headers["Cache-Control"] = "public,max-age=604800"
        });

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        // now register antiforgery _before_ mapping any endpoints
        app.UseAntiforgery();

        app.MapControllers();

        return app;
    }
}
