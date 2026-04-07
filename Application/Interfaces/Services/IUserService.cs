using Application.DTOs.Requests;
using Application.DTOs.Responses;

namespace Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<List<UserResponse>> GetAllAsync();
        Task<UserResponse?> GetByIdAsync(int id);
        Task<UserResponse?> GetByUserNameAsync(string userName);
        Task<int> CreateAsync(CreateUserRequest request);
        Task UpdateAsync(UpdateUserRequest request);
        Task DeleteAsync(int id);
        Task ActivateAsync(int id);
        Task DeactivateAsync(int id);
        Task ResetPasswordAsync(int id, string newPassword);
        Task AssignRoleAsync(int userId, string roleName);
        Task RemoveRoleAsync(int userId, string roleName);
    }
}
