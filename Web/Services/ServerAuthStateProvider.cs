using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace Web.Services;

public class ServerAuthStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CancellationTokenSource _cts = new();
    private Task<AuthenticationState>? _currentAuthenticationStateTask;

    public ServerAuthStateProvider(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor)
    {
        _serviceProvider = serviceProvider;
        _httpContextAccessor = httpContextAccessor;
        
        // Subscribe to circuit events
        var circuitAccessor = _serviceProvider.GetService<CircuitAccessor>();
        if (circuitAccessor != null)
        {
            circuitAccessor.CircuitClosed += OnCircuitClosed;
        }
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        Console.WriteLine($"[ServerAuthStateProvider] HttpContext: {httpContext != null}");

        if (httpContext == null)
        {
            Console.WriteLine($"[ServerAuthStateProvider] HttpContext is null");
            var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
            return Task.FromResult(new AuthenticationState(anonymous));
        }

        // Check HttpContext.User directly (set by authentication middleware)
        if (httpContext.User?.Identity?.IsAuthenticated == true)
        {
            Console.WriteLine($"[ServerAuthStateProvider] User authenticated via HttpContext.User: {httpContext.User.Identity.Name}");
            return Task.FromResult(new AuthenticationState(httpContext.User));
        }

        // Return unauthenticated state
        Console.WriteLine($"[ServerAuthStateProvider] User not authenticated");
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        return Task.FromResult(new AuthenticationState(anonymousUser));
    }

    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private void OnCircuitClosed(object? sender, EventArgs e)
    {
        _cts.Cancel();
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}

// Helper to access circuit
public class CircuitAccessor : CircuitHandler
{
    public event EventHandler? CircuitClosed;

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        CircuitClosed?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }
}
