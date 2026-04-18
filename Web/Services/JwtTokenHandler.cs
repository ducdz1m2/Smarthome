using Microsoft.JSInterop;

namespace Web.Services;

public class JwtTokenHandler
{
    private readonly IJSRuntime _jsRuntime;

    public JwtTokenHandler(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "JWTToken");
        }
        catch
        {
            // JS runtime not ready or localStorage not accessible
            return null;
        }
    }
}
