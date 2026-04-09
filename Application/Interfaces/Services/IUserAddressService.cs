using Application.DTOs.Requests;
using Application.DTOs.Responses;

namespace Application.Interfaces.Services
{
    public interface IUserAddressService
    {
        Task<UserAddressResponse?> GetByIdAsync(int id);
        Task<List<UserAddressResponse>> GetByUserIdAsync(int userId);
        Task<UserAddressResponse?> GetDefaultAsync(int userId);
        Task<UserAddressResponse> CreateAsync(int userId, CreateUserAddressRequest request);
        Task UpdateAsync(int id, CreateUserAddressRequest request);
        Task DeleteAsync(int id);
        Task SetAsDefaultAsync(int userId, int addressId);
    }
}
