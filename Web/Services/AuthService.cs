using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Application.Interfaces.Services;

namespace Web.Services;

public interface IAuthService
{
    Task SignInAsync(string userId, string userName, string email, string fullName, IEnumerable<string> roles, bool rememberMe = false);
    Task SignOutAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<string?> GetUserIdAsync();
    Task<string?> GetUserNameAsync();
    Task<IEnumerable<string>> GetRolesAsync();
    Task<int?> GetTechnicianIdAsync();
}

public class AuthService : IAuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly CurrentUserService _currentUserService;
    private readonly ITechnicianProfileService _technicianProfileService;

    public AuthService(IHttpContextAccessor httpContextAccessor, AuthenticationStateProvider authStateProvider, CurrentUserService currentUserService, ITechnicianProfileService technicianProfileService)
    {
        _httpContextAccessor = httpContextAccessor;
        _authStateProvider = authStateProvider;
        _currentUserService = currentUserService;
        _technicianProfileService = technicianProfileService;
    }

    public async Task SignInAsync(string userId, string userName, string email, string fullName, IEnumerable<string> roles, bool rememberMe = false)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        // Get technician ID if user is a technician
        int? technicianId = null;
        if (roles.Contains("Technician"))
        {
            try
            {
                var technicians = await _technicianProfileService.GetAvailableAsync();
                var technician = technicians.FirstOrDefault(t => t.UserId == int.Parse(userId));
                technicianId = technician?.Id;
            }
            catch
            {
                // Ignore errors - technicianId will remain null
            }
        }

        // Store user info in CurrentUserService for Blazor components
        _currentUserService.SetUser(int.Parse(userId), userName, email, fullName, roles, technicianId);

        // Only set cookie if response hasn't started
        if (!httpContext.Response.HasStarted)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Name, userName),
                new(ClaimTypes.Email, email),
                new(ClaimTypes.GivenName, fullName)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                authProperties);
        }

        // Notify Blazor authentication state changed
        if (_authStateProvider is ServerAuthStateProvider serverAuthStateProvider)
        {
            serverAuthStateProvider.NotifyAuthenticationStateChanged();
        }
    }

    public async Task SignOutAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // Clear user info from CurrentUserService
        _currentUserService.Clear();

        if (_authStateProvider is ServerAuthStateProvider serverAuthStateProvider)
        {
            serverAuthStateProvider.NotifyAuthenticationStateChanged();
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        return authState.User.Identity?.IsAuthenticated ?? false;
    }

    public async Task<string?> GetUserIdAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        return authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public async Task<string?> GetUserNameAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        return authState.User.Identity?.Name;
    }

    public async Task<IEnumerable<string>> GetRolesAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        return authState.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
    }

    public async Task<int?> GetTechnicianIdAsync()
    {
        Console.WriteLine($"[GetTechnicianIdAsync] Getting technician ID...");
        
        // First check if it's already in CurrentUserService (fast path)
        if (_currentUserService.TechnicianId.HasValue)
        {
            Console.WriteLine($"[GetTechnicianIdAsync] Found in CurrentUserService: {_currentUserService.TechnicianId.Value}");
            return _currentUserService.TechnicianId.Value;
        }

        Console.WriteLine($"[GetTechnicianIdAsync] Not in CurrentUserService, fetching from database...");

        // If not, reload from database
        var userId = await GetUserIdAsync();
        Console.WriteLine($"[GetTechnicianIdAsync] UserId: {userId}");
        
        if (userId == null)
        {
            Console.WriteLine($"[GetTechnicianIdAsync] UserId is null, returning null");
            return null;
        }

        try
        {
            var technicians = await _technicianProfileService.GetAvailableAsync();
            Console.WriteLine($"[GetTechnicianIdAsync] Found {technicians.Count} technicians");
            
            var technician = technicians.FirstOrDefault(t => t.UserId == int.Parse(userId));
            Console.WriteLine($"[GetTechnicianIdAsync] Technician found: {technician != null}, ID: {technician?.Id}");
            
            // Update CurrentUserService with the fetched technician ID
            if (technician != null)
            {
                _currentUserService.SetUser(
                    _currentUserService.UserId ?? 0,
                    _currentUserService.UserName ?? "",
                    _currentUserService.Email ?? "",
                    _currentUserService.FullName ?? "",
                    _currentUserService.Roles,
                    technician.Id
                );
                Console.WriteLine($"[GetTechnicianIdAsync] Updated CurrentUserService with TechnicianId: {technician.Id}");
            }
            
            return technician?.Id;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetTechnicianIdAsync] Error: {ex.Message}");
            return null;
        }
    }
}
