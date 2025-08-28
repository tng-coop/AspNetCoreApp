namespace BlazorWebApp.Services;

public interface IHostModeService
{
    ValueTask<bool> IsHostedOnWpAsync();
    ValueTask SetHostedOnWpAsync(bool value);
}
