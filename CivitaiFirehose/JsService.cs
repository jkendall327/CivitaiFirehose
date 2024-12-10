using CivitaiFirehose.Components.Pages;
using Microsoft.JSInterop;

namespace CivitaiFirehose;

public sealed class JsService(IJSRuntime js) : IDisposable
{
    private DotNetObjectReference<Home>? _dotNetReference;

    public async Task Initialise(Home component)
    {
        _dotNetReference = DotNetObjectReference.Create(component);
            
        await js.InvokeVoidAsync("eval", @"
            window.tabVisibilityHandler = {
                initialize: function (dotNetReference) {
                    document.addEventListener('visibilitychange', function () {
                        if (document.visibilityState === 'visible') {
                            dotNetReference.invokeMethodAsync('OnTabFocused');
                        }
                    });
                }
            };
        ");
            
        await js.InvokeVoidAsync("tabVisibilityHandler.initialize", _dotNetReference);
    }

    public async Task SetTabTitle(string title)
    {
        await js.InvokeVoidAsync("eval", $"document.title = '{title}'");
    }

    public void Dispose() => _dotNetReference?.Dispose();
}