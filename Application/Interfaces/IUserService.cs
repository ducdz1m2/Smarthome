using Application.DTOs;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllAsync();
        Task<UserDto?> GetByIdAsync(int id);
        Task<UserDto?> GetByEmailAsync(string email);
        Task<int> CreateAsync(CreateUserRequest request);
        Task UpdateAsync(int id, UpdateUserRequest request);
        Task DeleteAsync(int id);
        
        Task<List<UserAddressDto>> GetAddressesAsync(int userId);
        Task<int> AddAddressAsync(int userId, CreateUserAddressRequest request);
        Task UpdateAddressAsync(int addressId, CreateUserAddressRequest request);
        Task DeleteAddressAsync(int addressId);
        Task SetDefaultAddressAsync(int userId, int addressId);
    }
}
