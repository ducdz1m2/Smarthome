using Application.DTOs.Requests;
using Application.DTOs.Responses;

namespace Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse?> RefreshTokenAsync(string refreshToken);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task LogoutAsync(int userId);
        Task<UserResponse?> GetCurrentUserAsync(int userId);
    }
}
