using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Microsoft.AspNetCore.Identity;

namespace Application.Interfaces.Services
{
    public interface IIdentityService
    {
        // User Management
        Task<List<UserResponse>> GetAllUsersAsync();
        Task<UserResponse?> GetUserByIdAsync(int id);
        Task<UserResponse?> GetUserByNameAsync(string userName);
        Task<(IdentityResult Result, int UserId)> CreateUserAsync(CreateUserRequest request);
        Task<IdentityResult> UpdateUserAsync(UpdateUserRequest request);
        Task<IdentityResult> DeleteUserAsync(int id);
        Task<IdentityResult> ActivateUserAsync(int id);
        Task<IdentityResult> DeactivateUserAsync(int id);
        Task<IdentityResult> ResetPasswordAsync(int id, string newPassword);
        Task<IdentityResult> AssignRoleAsync(int userId, string roleName);
        Task<IdentityResult> RemoveRoleAsync(int userId, string roleName);
        Task<bool> UserExistsAsync(string userName);
        Task<bool> EmailExistsAsync(string email);

        // Role Management
        Task<List<RoleResponse>> GetAllRolesAsync();
        Task<RoleResponse?> GetRoleByIdAsync(int id);
        Task<RoleResponse?> GetRoleByNameAsync(string name);
        Task<IdentityResult> CreateRoleAsync(string name, string? description = null);
        Task<IdentityResult> UpdateRoleAsync(int id, string name, string? description = null);
        Task<IdentityResult> DeleteRoleAsync(int id);
        Task<bool> RoleExistsAsync(string name);

        // Authentication
        Task<AuthResponse?> LoginAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<IdentityResult> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<UserResponse?> GetCurrentUserAsync(int userId);
    }
}
