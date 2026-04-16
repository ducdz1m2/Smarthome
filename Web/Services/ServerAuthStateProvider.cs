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

        // Check HttpContext.User directly (set by JWT Bearer middleware)
        if (httpContext.User?.Identity?.IsAuthenticated == true)
        {
            Console.WriteLine($"[ServerAuthStateProvider] User authenticated via HttpContext.User: {httpContext.User.Identity.Name}");
            
            // Sync user data to CurrentUserService
            var currentUserService = _serviceProvider.GetService<CurrentUserService>();
            if (currentUserService != null)
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = httpContext.User.Identity.Name;
                var email = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
                var fullName = httpContext.User.FindFirst(ClaimTypes.GivenName)?.Value;
                var technicianIdClaim = httpContext.User.FindFirst("TechnicianId")?.Value;
                var roles = httpContext.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                
                if (int.TryParse(userIdClaim, out var userId))
                {
                    var technicianId = int.TryParse(technicianIdClaim, out var techId) ? techId : (int?)null;
                    currentUserService.SetUser(userId, userName ?? "", email ?? "", fullName ?? "", roles, technicianId);
                    Console.WriteLine($"[ServerAuthStateProvider] Synced user to CurrentUserService: UserId={userId}, UserName={userName}");
                }
            }
            
            return Task.FromResult(new AuthenticationState(httpContext.User));
        }

        // Return unauthenticated state and clear CurrentUserService
        Console.WriteLine($"[ServerAuthStateProvider] User not authenticated");
        var authService = _serviceProvider.GetService<CurrentUserService>();
        if (authService != null)
        {
            authService.Clear();
        }
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
