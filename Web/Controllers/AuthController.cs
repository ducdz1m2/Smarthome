using System.Security.Claims;
using Application.DTOs.Requests;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IIdentityService identityService, ILogger<AuthController> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _identityService.LoginAsync(request);

            // Create claims
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, response.User.Id.ToString()),
                new(ClaimTypes.Name, response.User.UserName),
                new(ClaimTypes.Email, response.User.Email),
                new(ClaimTypes.GivenName, response.User.FullName)
            };

            foreach (var role in response.User.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = request.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                authProperties);

            _logger.LogInformation("User {UserName} logged in successfully", response.User.UserName);

            return Ok(new
            {
                Success = true,
                User = response.User,
                RedirectUrl = response.User.Roles.Contains("Admin") ? "/admin" : "/"
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Login failed: {Message}", ex.Message);
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var response = await _identityService.RegisterAsync(request);

            // Create claims
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, response.User.Id.ToString()),
                new(ClaimTypes.Name, response.User.UserName),
                new(ClaimTypes.Email, response.User.Email),
                new(ClaimTypes.GivenName, response.User.FullName)
            };

            foreach (var role in response.User.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                authProperties);

            _logger.LogInformation("User {UserName} registered and logged in successfully", response.User.UserName);

            return Ok(new
            {
                Success = true,
                User = response.User,
                RedirectUrl = "/"
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Registration failed: {Message}", ex.Message);
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _logger.LogInformation("User logged out");
        return Ok(new { Success = true });
    }
}
