using Application.DTOs.Requests;
using Application.DTOs.Responses;

namespace Application.Interfaces.Services
{
    public interface IWarehouseService
    {
        Task<List<WarehouseResponse>> GetAllAsync();
        Task<WarehouseResponse?> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateWarehouseRequest request);
        Task UpdateAsync(int id, UpdateWarehouseRequest request);
        Task DeleteAsync(int id);
        Task<bool> ActivateAsync(int id);
        Task<bool> DeactivateAsync(int id);
    }
}
