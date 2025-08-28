using Microsoft.JSInterop;

namespace BlazorWebApp.Services;

public sealed class HostModeService : IHostModeService
{
    private readonly IJSRuntime _js;
    private bool? _cached;

    public HostModeService(IJSRuntime js)
    {
        _js = js;
    }

    public async ValueTask<bool> IsHostedOnWpAsync()
    {
        if (_cached is not null)
        {
            return _cached.Value;
        }

        var stored = await _js.InvokeAsync<string>("localStorage.getItem", "hostInWp");
        if (bool.TryParse(stored, out var value))
        {
            _cached = value;
            return value;
        }

        var auto = await _js.InvokeAsync<bool>("isRunningInWp");
        await _js.InvokeVoidAsync("localStorage.setItem", "hostInWp", auto ? "true" : "false");
        _cached = auto;
        return auto;
    }

    public async ValueTask SetHostedOnWpAsync(bool value)
    {
        _cached = value;
        await _js.InvokeVoidAsync("localStorage.setItem", "hostInWp", value ? "true" : "false");
    }
}
