using System.Globalization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;
using BlazorWebApp.Data;

namespace BlazorWebApp.Services;

public class LocalizationService
{
    private readonly IJSRuntime _js;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly UserManager<ApplicationUser> _userManager;

    public event Action? OnChange;

    public LocalizationService(
        IJSRuntime js,
        AuthenticationStateProvider authStateProvider,
        UserManager<ApplicationUser> userManager)
    {
        _js = js;
        _authStateProvider = authStateProvider;
        _userManager = userManager;
    }

    public CultureInfo CurrentCulture { get; private set; } = new("en");

    public async Task InitializeAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        string culture;

        if (user.Identity?.IsAuthenticated == true)
        {
            var appUser = await _userManager.GetUserAsync(user);
            culture = appUser?.PreferredLanguage ?? "en";
        }
        else
        {
            culture = await _js.InvokeAsync<string>("blazorCulture.get") ?? "en";
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
            var appUser = await _userManager.GetUserAsync(user);
            if (appUser != null && appUser.PreferredLanguage != culture)
            {
                appUser.PreferredLanguage = culture;
                await _userManager.UpdateAsync(appUser);
            }
        }
        else
        {
            await _js.InvokeVoidAsync("blazorCulture.set", culture);
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
