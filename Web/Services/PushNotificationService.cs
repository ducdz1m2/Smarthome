using Microsoft.JSInterop;

namespace Web.Services;

public class PushNotificationService
{
    private readonly IJSRuntime _jsRuntime;

    public PushNotificationService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<bool> InitAsync()
    {
        try
        {
            // Script is now loaded directly in App.razor, just check if it's available
            var isLoaded = await _jsRuntime.InvokeAsync<bool>("eval", @"
                (function() {
                    return typeof window.PushNotificationService !== 'undefined';
                })();
            ");

            if (!isLoaded)
            {
                Console.WriteLine("[PushNotificationService] Script not loaded");
                return false;
            }

            return await _jsRuntime.InvokeAsync<bool>("PushNotificationService.init");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PushNotificationService] Init error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SubscribeAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("PushNotificationService.subscribe");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PushNotificationService] Subscribe error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UnsubscribeAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("PushNotificationService.unsubscribe");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PushNotificationService] Unsubscribe error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> IsSubscribedAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("PushNotificationService.isSubscribed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PushNotificationService] IsSubscribed error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> IsSupportedAsync()
    {
        try
        {
            var result = await _jsRuntime.InvokeAsync<bool>("eval", @"
                (function() {
                    console.log('[PushNotificationService] Checking support...');
                    console.log('[PushNotificationService] serviceWorker in navigator:', 'serviceWorker' in navigator);
                    console.log('[PushNotificationService] PushManager in window:', 'PushManager' in window);
                    console.log('[PushNotificationService] navigator:', navigator);
                    console.log('[PushNotificationService] window:', window);
                    return 'serviceWorker' in navigator && 'PushManager' in window;
                })();
            ");
            Console.WriteLine($"[PushNotificationService] IsSupported result: {result}");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PushNotificationService] IsSupported error: {ex.Message}");
            return false;
        }
    }
}
