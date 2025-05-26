using System.Globalization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using BlazorWebApp.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace BlazorWebApp.Services;

public class LocalizationService
{
    private readonly ProtectedLocalStorage _storage;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IJSRuntime _jsRuntime;

    public event Action? OnChange;

    public LocalizationService(
        ProtectedLocalStorage storage,
        AuthenticationStateProvider authStateProvider,
        IServiceScopeFactory scopeFactory,
        IJSRuntime jsRuntime)
    {
        _storage = storage;
        _authStateProvider = authStateProvider;
        _scopeFactory = scopeFactory;
        _jsRuntime = jsRuntime;
        CurrentCulture = CultureInfo.CurrentCulture;
    }

    public CultureInfo CurrentCulture { get; private set; }

    public async Task LoadCultureAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        string culture;

        if (user.Identity?.IsAuthenticated == true)
        {
            using var scope = _scopeFactory.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var appUser = await userManager.GetUserAsync(user);
            culture = appUser?.PreferredLanguage ?? "en";
        }
        else
        {
            var result = await _storage.GetAsync<string>("blazorCulture");
            if (result.Success)
            {
                culture = result.Value;
            }
            else
            {
                var browserLang = await _jsRuntime.InvokeAsync<string>("localization.getPreferredLanguage");
                culture = !string.IsNullOrEmpty(browserLang) && browserLang.StartsWith("ja", StringComparison.OrdinalIgnoreCase)
                    ? "ja"
                    : "en";
            }
        }

        SetThreadCulture(culture);
        OnChange?.Invoke();
    }

    public async Task SetCultureAsync(string culture)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        SetThreadCulture(culture);

        if (user.Identity?.IsAuthenticated == true)
        {
            using var scope = _scopeFactory.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var appUser = await userManager.GetUserAsync(user);
            if (appUser != null && appUser.PreferredLanguage != culture)
            {
                appUser.PreferredLanguage = culture;
                await userManager.UpdateAsync(appUser);
            }
        }
        else
        {
            await _storage.SetAsync("blazorCulture", culture);
        }

        OnChange?.Invoke();
    }

    private void SetThreadCulture(string culture)
    {
        var cultureInfo = new CultureInfo(culture);
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        CurrentCulture = cultureInfo;
    }
}
