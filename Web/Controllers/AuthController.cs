using Application.DTOs.Requests;
using Application.Interfaces.Services;
using Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IIdentityService _identityService;
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IIdentityService identityService, IAuthService authService, ILogger<AuthController> logger)
    {
        _identityService = identityService;
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _identityService.LoginAsync(request);

            _logger.LogInformation("User {UserName} logged in successfully", response.User.UserName);

            // Determine redirect URL based on role
            string redirectUrl = "/";
            if (response.User.Roles.Contains("Admin"))
                redirectUrl = "/admin";
            else if (response.User.Roles.Contains("Technician"))
                redirectUrl = "/technician";

            return Ok(new
            {
                Success = true,
                Token = response.Token,
                ExpiresAt = response.ExpiresAt,
                User = response.User,
                RedirectUrl = redirectUrl
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

            _logger.LogInformation("User {UserName} registered and logged in successfully", response.User.UserName);

            return Ok(new
            {
                Success = true,
                Token = response.Token,
                ExpiresAt = response.ExpiresAt,
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
        await _authService.SignOutAsync();
        _logger.LogInformation("User logged out");
        return Ok(new { Success = true });
    }
}
