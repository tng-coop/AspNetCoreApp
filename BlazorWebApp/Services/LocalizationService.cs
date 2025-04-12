using System.Globalization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using BlazorWebApp.Data;

namespace BlazorWebApp.Services;

public class LocalizationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly UserManager<ApplicationUser> _userManager;

    public event Action? OnChange;

    public LocalizationService(
        IHttpContextAccessor httpContextAccessor,
        AuthenticationStateProvider authStateProvider,
        UserManager<ApplicationUser> userManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _authStateProvider = authStateProvider;
        _userManager = userManager;
    }

    public CultureInfo CurrentCulture => CultureInfo.CurrentCulture;

    public async Task InitializeUserCultureAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        var culture = "en"; // default culture

        if (user.Identity?.IsAuthenticated == true)
        {
            var appUser = await _userManager.GetUserAsync(user);
            culture = appUser?.PreferredLanguage ?? "en";
        }
        else
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                var feature = httpContext.Features.Get<IRequestCultureFeature>();
                culture = feature?.RequestCulture.Culture.Name ?? "en";
            }
        }

        SetThreadCulture(culture);
        OnChange?.Invoke();
    }

    public async Task SetCultureAsync(string culture)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null) return;

        // Set the culture cookie using ASP.NET Core's built-in provider
        httpContext.Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });

        SetThreadCulture(culture);

        // Save to DB if user is authenticated
        if (user.Identity?.IsAuthenticated == true)
        {
            var appUser = await _userManager.GetUserAsync(user);
            if (appUser != null)
            {
                appUser.PreferredLanguage = culture;
                await _userManager.UpdateAsync(appUser);
            }
        }

        OnChange?.Invoke();
    }

    private void SetThreadCulture(string culture)
    {
        var cultureInfo = new CultureInfo(culture);
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
    }
}
