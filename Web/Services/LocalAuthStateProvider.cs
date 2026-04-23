using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;

namespace Web.Services;

public class LocalAuthStateProvider : AuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CurrentUserService _currentUserService;
    private readonly IJSRuntime _jsRuntime;

    public LocalAuthStateProvider(IHttpContextAccessor httpContextAccessor, CurrentUserService currentUserService, IJSRuntime jsRuntime)
    {
        _httpContextAccessor = httpContextAccessor;
        _currentUserService = currentUserService;
        _jsRuntime = jsRuntime;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        string? token = null;
        
        // Try to read from session first
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            token = httpContext.Session.GetString("JWTToken");
        }

        // If not in session, try localStorage
        if (string.IsNullOrEmpty(token))
        {
            try
            {
                token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "JWTToken");
            }
            catch (Microsoft.JSInterop.JSDisconnectedException)
            {
                // Circuit is disconnecting, return anonymous
                _currentUserService.Clear();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            catch (Microsoft.JSInterop.JSException)
            {
                // JS interop not available (prerendering), return anonymous
                _currentUserService.Clear();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            catch (InvalidOperationException)
            {
                // Invalid operation during prerendering, return anonymous
                _currentUserService.Clear();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            catch (Exception)
            {
                // Error reading from localStorage
            }
        }

        if (string.IsNullOrEmpty(token))
        {
            _currentUserService.Clear();
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            // Extract claims from JWT token
            var claims = jwtToken.Claims.ToList();
            
            var identity = new ClaimsIdentity(claims, "Jwt");
            var principal = new ClaimsPrincipal(identity);

            // Sync user data to CurrentUserService
            // JWT uses custom claim types: nameid, unique_name, email, given_name, role
            var userIdClaim = principal.FindFirst("nameid")?.Value;
            var userName = principal.FindFirst("unique_name")?.Value;
            var email = principal.FindFirst("email")?.Value;
            var fullName = principal.FindFirst("given_name")?.Value;
            var roles = principal.FindAll("role").Select(c => c.Value).ToList();
            var technicianIdClaim = principal.FindFirst("TechnicianId")?.Value;
            int? technicianId = null;
            if (!string.IsNullOrEmpty(technicianIdClaim) && int.TryParse(technicianIdClaim, out var tid))
            {
                technicianId = tid;
            }


            if (int.TryParse(userIdClaim, out var userId))
            {
                _currentUserService.SetUser(userId, userName ?? "", email ?? "", fullName ?? "", roles, technicianId);
            }

            return new AuthenticationState(principal);
        }
        catch (Exception)
        {
            _currentUserService.Clear();
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task<string?> GetTokenAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            var token = httpContext.Session.GetString("JWTToken");
            if (!string.IsNullOrEmpty(token)) return token;
        }

        try
        {
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "JWTToken");
        }
        catch
        {
            return null;
        }
    }
}
