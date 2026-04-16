using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Application.Interfaces.Services;
using Microsoft.JSInterop;

namespace Web.Services;

public interface IAuthService
{
    Task<string?> GetTokenAsync();
    Task SetTokenAsync(string token);
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
    private readonly IJSRuntime _jsRuntime;

    public AuthService(IHttpContextAccessor httpContextAccessor, AuthenticationStateProvider authStateProvider, CurrentUserService currentUserService, ITechnicianProfileService technicianProfileService, IJSRuntime jsRuntime)
    {
        _httpContextAccessor = httpContextAccessor;
        _authStateProvider = authStateProvider;
        _currentUserService = currentUserService;
        _technicianProfileService = technicianProfileService;
        _jsRuntime = jsRuntime;
    }

    public async Task<string?> GetTokenAsync()
    {
        // Read from session for LocalAuthStateProvider
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            return httpContext.Session.GetString("JWTToken");
        }
        return null;
    }

    public async Task SetTokenAsync(string token)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null && !httpContext.Response.HasStarted)
        {
            // Save to session for LocalAuthStateProvider (only if response hasn't started)
            httpContext.Session.SetString("JWTToken", token);
        }

        // Always save to localStorage for HttpClient (JwtTokenMessageHandler)
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "JWTToken", token);

        // Notify Blazor authentication state changed
        if (_authStateProvider is LocalAuthStateProvider localAuthStateProvider)
        {
            localAuthStateProvider.NotifyAuthenticationStateChanged();
        }
    }

    public async Task SignOutAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            // Remove from session
            httpContext.Session.Remove("JWTToken");
        }

        // Remove from localStorage
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "JWTToken");

        // Clear user info from CurrentUserService
        _currentUserService.Clear();

        if (_authStateProvider is LocalAuthStateProvider localAuthStateProvider)
        {
            localAuthStateProvider.NotifyAuthenticationStateChanged();
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrEmpty(token);
    }

    public async Task<string?> GetUserIdAsync()
    {
        return _currentUserService.UserId?.ToString();
    }

    public async Task<string?> GetUserNameAsync()
    {
        return _currentUserService.UserName;
    }

    public async Task<IEnumerable<string>> GetRolesAsync()
    {
        return _currentUserService.Roles ?? Enumerable.Empty<string>();
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

        // If not, try to fetch it
        if (_currentUserService.UserId.HasValue)
        {
            try
            {
                var technicians = await _technicianProfileService.GetAvailableAsync();
                var technician = technicians.FirstOrDefault(t => t.UserId == _currentUserService.UserId.Value);
                if (technician != null)
                {
                    Console.WriteLine($"[GetTechnicianIdAsync] Found in database: {technician.Id}");
                    _currentUserService.TechnicianId = technician.Id;
                    return technician.Id;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetTechnicianIdAsync] Error: {ex.Message}");
            }
        }

        Console.WriteLine($"[GetTechnicianIdAsync] Not found");
        return null;
    }
}
