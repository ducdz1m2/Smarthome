using Domain.Entities.Common;
using Domain.Exceptions;

namespace Domain.Entities.Inventory
{
    public class Warehouse : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string Code { get; private set; } = string.Empty;
        public string AddressStreet { get; private set; } = null!;
        public string? AddressWard { get; private set; }
        public string? AddressDistrict { get; private set; }
        public string? AddressCity { get; private set; }
        public string? Phone { get; private set; }
        public string? ManagerName { get; private set; }
        public bool IsActive { get; private set; } = true;

        // Navigation
        public virtual ICollection<ProductWarehouse> ProductWarehouses { get; private set; } = new List<ProductWarehouse>();

        private Warehouse() { } // EF Core

        public static Warehouse Create(string name, string code, string addressStreet, string? addressWard = null, string? addressDistrict = null, string? addressCity = null,
            string? phone = null, string? managerName = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Tên kho không được trống");

            if (string.IsNullOrWhiteSpace(code))
                throw new DomainException("Mã kho không được trống");

            if (code.Length < 3 || code.Length > 20)
                throw new DomainException("Mã kho phải từ 3-20 ký tự");

            return new Warehouse
            {
                Name = name.Trim(),
                Code = code.Trim().ToUpper(),
                AddressStreet = addressStreet.Trim(),
                AddressWard = addressWard?.Trim(),
                AddressDistrict = addressDistrict?.Trim(),
                AddressCity = addressCity?.Trim(),
                Phone = phone?.Trim(),
                ManagerName = managerName?.Trim(),
                IsActive = true
            };
        }

        public void Update(string name, string addressStreet, string? addressWard, string? addressDistrict, string? addressCity, string? phone, string? managerName)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Tên kho không được trống");

            Name = name.Trim();
            AddressStreet = addressStreet.Trim();
            AddressWard = addressWard?.Trim();
            AddressDistrict = addressDistrict?.Trim();
            AddressCity = addressCity?.Trim();
            Phone = phone?.Trim();
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
}
