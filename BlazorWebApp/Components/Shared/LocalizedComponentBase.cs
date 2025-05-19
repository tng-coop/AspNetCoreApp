using Microsoft.AspNetCore.Components;
using BlazorWebApp.Services;

namespace BlazorWebApp.Components.Shared;

public class LocalizedComponentBase : ComponentBase, IDisposable
{
    [Inject]
    protected LocalizationService Localization { get; set; } = default!;

    private bool _initialized;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_initialized)
        {
            await Localization.LoadCultureAsync();
            Localization.OnChange += StateHasChanged;
            _initialized = true;
            StateHasChanged();
        }
    }

    public void Dispose()
    {
        Localization.OnChange -= StateHasChanged;
    }
}
