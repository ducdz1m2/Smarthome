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
        Console.WriteLine($"[LocalAuthStateProvider] GetAuthenticationStateAsync called");
        
        string? token = null;
        
        // Try to read from session first
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            token = httpContext.Session.GetString("JWTToken");
            Console.WriteLine($"[LocalAuthStateProvider] Token from session: {(token != null ? "exists" : "null")}");
        }

        // If not in session, try localStorage
        if (string.IsNullOrEmpty(token))
        {
            try
            {
                token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "JWTToken");
                Console.WriteLine($"[LocalAuthStateProvider] Token from localStorage: {(token != null ? "exists" : "null")}");
            }
            catch (Microsoft.JSInterop.JSDisconnectedException)
            {
                // Circuit is disconnecting, return anonymous
                Console.WriteLine($"[LocalAuthStateProvider] Circuit disconnected, returning anonymous");
                _currentUserService.Clear();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            catch (Microsoft.JSInterop.JSException)
            {
                // JS interop not available (prerendering), return anonymous
                Console.WriteLine($"[LocalAuthStateProvider] JS interop not available (prerendering), returning anonymous");
                _currentUserService.Clear();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            catch (InvalidOperationException)
            {
                // Invalid operation during prerendering, return anonymous
                Console.WriteLine($"[LocalAuthStateProvider] Invalid operation (prerendering), returning anonymous");
                _currentUserService.Clear();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalAuthStateProvider] Error reading from localStorage: {ex.Message}");
            }
        }

        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine($"[LocalAuthStateProvider] No token found, returning anonymous");
            _currentUserService.Clear();
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            // Extract claims from JWT token
            var claims = jwtToken.Claims.ToList();
            
            // Debug: log all claims
            Console.WriteLine($"[LocalAuthStateProvider] JWT Claims count: {claims.Count}");
            foreach (var claim in claims)
            {
                Console.WriteLine($"[LocalAuthStateProvider] Claim: {claim.Type} = {claim.Value}");
            }
            
            var identity = new ClaimsIdentity(claims, "Jwt");
            var principal = new ClaimsPrincipal(identity);

            Console.WriteLine($"[LocalAuthStateProvider] User authenticated: {principal.Identity?.Name}");

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

            Console.WriteLine($"[LocalAuthStateProvider] Extracted: UserIdClaim={userIdClaim}, UserName={userName}, Email={email}, FullName={fullName}, Roles=[{string.Join(", ", roles)}], TechnicianId={technicianId}");

            if (int.TryParse(userIdClaim, out var userId))
            {
                _currentUserService.SetUser(userId, userName ?? "", email ?? "", fullName ?? "", roles, technicianId);
                Console.WriteLine($"[LocalAuthStateProvider] Synced to CurrentUserService: UserId={userId}, UserName={userName}, TechnicianId={technicianId}");
            }
            else
            {
                Console.WriteLine($"[LocalAuthStateProvider] Failed to parse UserIdClaim: {userIdClaim}");
            }

            return new AuthenticationState(principal);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LocalAuthStateProvider] Error parsing JWT: {ex.Message}");
            _currentUserService.Clear();
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public void NotifyAuthenticationStateChanged()
    {
        Console.WriteLine($"[LocalAuthStateProvider] NotifyAuthenticationStateChanged called");
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
