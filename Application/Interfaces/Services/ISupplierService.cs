using Application.DTOs.Requests;
using Application.DTOs.Responses;

namespace Application.Interfaces.Services
{
    public interface ISupplierService
    {
        Task<List<SupplierResponse>> GetAllAsync();
        Task<SupplierResponse?> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateSupplierRequest request);
        Task UpdateAsync(int id, UpdateSupplierRequest request);
        Task DeleteAsync(int id);
        Task<bool> ActivateAsync(int id);
        Task<bool> DeactivateAsync(int id);
    }
}
