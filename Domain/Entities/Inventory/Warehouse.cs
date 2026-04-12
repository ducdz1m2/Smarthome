using Domain.Abstractions;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities.Inventory;

/// <summary>
/// Warehouse aggregate root - represents a storage location for inventory.
/// </summary>
public class Warehouse : AggregateRoot
    {
        public string Name { get; private set; } = string.Empty;
        public string Code { get; private set; } = string.Empty;
        public Address Address { get; private set; } = null!;
        public PhoneNumber? Phone { get; private set; }
        public string? ManagerName { get; private set; }
        public bool IsActive { get; private set; } = true;

        // Navigation
        public virtual ICollection<ProductWarehouse> ProductWarehouses { get; private set; } = new List<ProductWarehouse>();

        private Warehouse() { } // EF Core

        public static Warehouse Create(string name, string code, Address address, PhoneNumber? phone = null, string? managerName = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException(nameof(name), "Tên kho không được trống");

            if (string.IsNullOrWhiteSpace(code))
                throw new ValidationException(nameof(code), "Mã kho không được trống");

            if (code.Length < 3 || code.Length > 20)
                throw new ValidationException(nameof(code), "Mã kho phải từ 3-20 ký tự");

            return new Warehouse
            {
                Name = name.Trim(),
                Code = code.Trim().ToUpper(),
                Address = address,
                Phone = phone,
                ManagerName = managerName?.Trim(),
                IsActive = true
            };
        }

        // Legacy overload for backward compatibility
        public static Warehouse Create(string name, string code, string addressStreet, string? addressWard = null, string? addressDistrict = null, string? addressCity = null,
            string? phone = null, string? managerName = null)
        {
            var address = Address.Create(addressStreet, addressWard, addressDistrict ?? "", addressCity ?? "");
            var phoneVo = string.IsNullOrWhiteSpace(phone) ? null : PhoneNumber.Create(phone);
            return Create(name, code, address, phoneVo, managerName);
        }

        public void Update(string name, Address address, PhoneNumber? phone, string? managerName)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException(nameof(name), "Tên kho không được trống");

            Name = name.Trim();
            Address = address;
            Phone = phone;
            ManagerName = managerName?.Trim();
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public bool HasStock() => ProductWarehouses.Any(pw => pw.Quantity > 0);
    }
