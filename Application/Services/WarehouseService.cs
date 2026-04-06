using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Inventory;
using Domain.Exceptions;

namespace Application.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IWarehouseRepository _warehouseRepository;

        public WarehouseService(IWarehouseRepository warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        public async Task<List<WarehouseResponse>> GetAllAsync()
        {
            var warehouses = await _warehouseRepository.GetAllAsync();
            return warehouses.Select(MapToResponse).ToList();
        }

        public async Task<WarehouseResponse?> GetByIdAsync(int id)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(id);
            if (warehouse == null) return null;
            return MapToResponse(warehouse);
        }

        public async Task<int> CreateAsync(CreateWarehouseRequest request)
        {
            if (await _warehouseRepository.ExistsAsync(request.Name))
                throw new DomainException("Tên kho đã tồn tại");

            if (await _warehouseRepository.CodeExistsAsync(request.Code))
                throw new DomainException("Mã kho đã tồn tại");

            var warehouse = Warehouse.Create(
                request.Name,
                request.Code,
                request.AddressStreet ?? "",
                request.AddressWard,
                request.AddressDistrict,
                request.AddressCity,
                request.Phone
            );

            await _warehouseRepository.AddAsync(warehouse);
            await _warehouseRepository.SaveChangesAsync();
            return warehouse.Id;
        }

        public async Task UpdateAsync(int id, UpdateWarehouseRequest request)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(id);
            if (warehouse == null)
                throw new DomainException("Không tìm thấy kho");

            if (await _warehouseRepository.ExistsAsync(request.Name, id))
                throw new DomainException("Tên kho đã tồn tại");

            warehouse.Update(
                request.Name,
                request.AddressStreet ?? "",
                request.AddressWard,
                request.AddressDistrict,
                request.AddressCity,
                request.Phone,
                request.ManagerName
            );

            if (request.IsActive && !warehouse.IsActive)
                warehouse.Activate();
            else if (!request.IsActive && warehouse.IsActive)
                warehouse.Deactivate();

            _warehouseRepository.Update(warehouse);
            await _warehouseRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(id);
            if (warehouse == null)
                throw new DomainException("Không tìm thấy kho");

            if (warehouse.HasStock())
                throw new DomainException("Không thể xóa kho đang có tồn kho");

            _warehouseRepository.Delete(warehouse);
            await _warehouseRepository.SaveChangesAsync();
        }

        public async Task<bool> ActivateAsync(int id)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(id);
            if (warehouse == null) return false;

            warehouse.Activate();
            _warehouseRepository.Update(warehouse);
            await _warehouseRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(id);
            if (warehouse == null) return false;

            warehouse.Deactivate();
            _warehouseRepository.Update(warehouse);
            await _warehouseRepository.SaveChangesAsync();
            return true;
        }

        private WarehouseResponse MapToResponse(Warehouse warehouse)
        {
            var address = string.Join(", ", new[] { warehouse.AddressStreet, warehouse.AddressWard, warehouse.AddressDistrict, warehouse.AddressCity }
                .Where(s => !string.IsNullOrWhiteSpace(s)));

            return new WarehouseResponse
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Code = warehouse.Code,
                Address = address,
                AddressStreet = warehouse.AddressStreet,
                AddressWard = warehouse.AddressWard,
                AddressDistrict = warehouse.AddressDistrict,
                AddressCity = warehouse.AddressCity,
                Phone = warehouse.Phone,
                ManagerName = warehouse.ManagerName,
                IsActive = warehouse.IsActive
            };
        }
    }
}
