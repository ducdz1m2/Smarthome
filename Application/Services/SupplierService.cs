using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Inventory;
using Domain.Exceptions;

namespace Application.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _supplierRepository;

        public SupplierService(ISupplierRepository supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }

        public async Task<List<SupplierResponse>> GetAllAsync()
        {
            var suppliers = await _supplierRepository.GetAllAsync();
            return suppliers.Select(MapToResponse).ToList();
        }

        public async Task<SupplierResponse?> GetByIdAsync(int id)
        {
            var supplier = await _supplierRepository.GetByIdWithStockEntriesAsync(id);
            if (supplier == null) return null;
            return MapToResponse(supplier);
        }

        public async Task<int> CreateAsync(CreateSupplierRequest request)
        {
            if (await _supplierRepository.ExistsAsync(request.Name))
                throw new DomainException("Tên nhà cung cấp đã tồn tại");

            var supplier = Supplier.Create(
                request.Name,
                request.TaxCode,
                request.AddressStreet,
                request.AddressWard,
                request.AddressDistrict,
                request.AddressCity,
                request.ContactName,
                request.Phone,
                request.Email
            );

            if (!string.IsNullOrWhiteSpace(request.BankAccount) || !string.IsNullOrWhiteSpace(request.BankName))
            {
                supplier.UpdateBankInfo(request.BankAccount ?? "", request.BankName ?? "");
            }

            await _supplierRepository.AddAsync(supplier);
            await _supplierRepository.SaveChangesAsync();
            return supplier.Id;
        }

        public async Task UpdateAsync(int id, UpdateSupplierRequest request)
        {
            var supplier = await _supplierRepository.GetByIdAsync(id);
            if (supplier == null)
                throw new DomainException("Không tìm thấy nhà cung cấp");

            if (await _supplierRepository.ExistsAsync(request.Name, id))
                throw new DomainException("Tên nhà cung cấp đã tồn tại");

            supplier.Update(
                request.Name, 
                request.ContactName, 
                request.Phone != null ? Domain.ValueObjects.PhoneNumber.Create(request.Phone) : null,
                request.Email != null ? Domain.ValueObjects.Email.Create(request.Email) : null);

            if (request.IsActive && !supplier.IsActive)
                supplier.Activate();
            else if (!request.IsActive && supplier.IsActive)
                supplier.Deactivate();

            _supplierRepository.Update(supplier);
            await _supplierRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var supplier = await _supplierRepository.GetByIdWithStockEntriesAsync(id);
            if (supplier == null)
                throw new DomainException("Không tìm thấy nhà cung cấp");

            if (supplier.StockEntries?.Any() == true)
                throw new DomainException("Không thể xóa nhà cung cấp đã có phiếu nhập kho");

            _supplierRepository.Delete(supplier);
            await _supplierRepository.SaveChangesAsync();
        }

        public async Task<bool> ActivateAsync(int id)
        {
            var supplier = await _supplierRepository.GetByIdAsync(id);
            if (supplier == null) return false;

            supplier.Activate();
            _supplierRepository.Update(supplier);
            await _supplierRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var supplier = await _supplierRepository.GetByIdAsync(id);
            if (supplier == null) return false;

            supplier.Deactivate();
            _supplierRepository.Update(supplier);
            await _supplierRepository.SaveChangesAsync();
            return true;
        }

        private SupplierResponse MapToResponse(Supplier supplier)
        {
            var address = string.Join(", ", new[]
            {
                supplier.Address?.Street,
                supplier.Address?.Ward,
                supplier.Address?.District,
                supplier.Address?.City
            }.Where(s => !string.IsNullOrWhiteSpace(s)));

            return new SupplierResponse
            {
                Id = supplier.Id,
                Name = supplier.Name,
                TaxCode = supplier.TaxCode,
                Address = address,
                ContactName = supplier.ContactName,
                Phone = supplier.Phone?.ToString(),
                Email = supplier.Email?.ToString(),
                BankAccount = supplier.BankAccount,
                BankName = supplier.BankName,
                IsActive = supplier.IsActive,
                StockEntryCount = supplier.StockEntries?.Count ?? 0,
                CreatedAt = supplier.CreatedAt
            };
        }
    }
}
