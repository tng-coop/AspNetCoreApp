using Bunit;
using BlazorWebApp.Components.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using BlazorWebApp.Services;
using System.Globalization;
using System.Collections.Generic;
using System.Security.Claims;
using Xunit;

public class CounterComponentTests : TestContext
{
    private class DummyStringLocalizer<T> : IStringLocalizer<T>
    {
        public LocalizedString this[string name] => new(name, name);
        public LocalizedString this[string name, params object[] arguments]
            => new(name, string.Format(name, arguments));
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => new List<LocalizedString>();
        public IStringLocalizer WithCulture(CultureInfo culture) => this;
    }

    private class DummyJSRuntime : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => new(default(TValue)!);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => new(default(TValue)!);
    }

    private class DummyAuthStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
            => Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
    }

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
    [Fact]
    public void CounterIncrementsWhenClicked()
    {
        Services.AddSingleton<IStringLocalizer<Counter>>(new DummyStringLocalizer<Counter>());
        Services.AddSingleton<IJSRuntime>(new DummyJSRuntime());
        Services.AddSingleton<AuthenticationStateProvider>(new DummyAuthStateProvider());
        Services.AddSingleton<IServiceScopeFactory>(sp => new DummyScopeFactory(sp));
        Services.AddScoped<LocalizationService>();

        // Act
        var cut = RenderComponent<Counter>();

        // Assert initial state
        Assert.Contains("Current count: 0", cut.Markup);

        // Click the first button to increment the count
        cut.Find("button").Click();

        // Assert count has incremented
        Assert.Contains("Current count: 1", cut.Markup);
    }
}
