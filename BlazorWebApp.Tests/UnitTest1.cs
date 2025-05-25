using Bunit;
using BlazorWebApp.Components.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using BlazorWebApp.Services;
using System.Globalization;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class CounterComponentTests : TestContext
{
    // JSInterop stub
    private class DummyJSRuntime : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
            => new(default(TValue)!);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
            => new(default(TValue)!);
    }

    // Authentication state stub
    private class DummyAuthStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
            => Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
    }

    // Scope factory stub for localization
    private class DummyScopeFactory : IServiceScopeFactory
    {
        private readonly IServiceProvider _provider;
        public DummyScopeFactory(IServiceProvider provider) => _provider = provider;
        public IServiceScope CreateScope() => new DummyScope(_provider);

        private class DummyScope : IServiceScope
        {
            public DummyScope(IServiceProvider provider) => ServiceProvider = provider;
            public IServiceProvider ServiceProvider { get; }
            public void Dispose() { }
        }
    }

    public CounterComponentTests()
    {
        // Localization: point to Resources folder and register services
        Services.AddLocalization(options => options.ResourcesPath = "Resources");
        // Ensure culture is consistent
        CultureInfo.DefaultThreadCurrentCulture   = new CultureInfo("en-US");
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

        // If AddLocalization doesn't wire up the generic localizer, register it:
        Services.AddSingleton(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));

        // Register other dependencies
        Services.AddSingleton<IJSRuntime>(new DummyJSRuntime());
        Services.AddSingleton<AuthenticationStateProvider>(new DummyAuthStateProvider());
        Services.AddSingleton<IServiceScopeFactory>(sp => new DummyScopeFactory(sp));
        Services.AddScoped<LocalizationService>();
    }

    [Fact]
    public void CounterIncrementsWhenClicked()
    {
        // Act
        var cut = RenderComponent<Counter>();

        // Assert initial state is zero
        Assert.Contains("Current count: 0", cut.Markup);

        // Perform click
        cut.Find("button").Click();

        // After click, count should be one
        Assert.Contains("Current count: 1", cut.Markup);
    }
}
