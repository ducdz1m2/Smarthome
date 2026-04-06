namespace Domain.Entities.Inventory
{
    using Domain.Entities.Common;
    using Domain.Exceptions;

    public class Supplier : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string? TaxCode { get; private set; }
        public string? AddressStreet { get; private set; }
        public string? AddressWard { get; private set; }
        public string? AddressDistrict { get; private set; }
        public string? AddressCity { get; private set; }
        public string? ContactName { get; private set; }
        public string? Phone { get; private set; }
        public string? Email { get; private set; }
        public string? BankAccount { get; private set; }
        public string? BankName { get; private set; }
        public bool IsActive { get; private set; } = true;

        public virtual ICollection<StockEntry> StockEntries { get; private set; } = new List<StockEntry>();

        private Supplier() { }

        public static Supplier Create(string name, string? taxCode = null, string? addressStreet = null, string? addressWard = null, string? addressDistrict = null, string? addressCity = null,
            string? contactName = null, string? phone = null, string? email = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Tên nhà cung cấp không được trống");

            return new Supplier
            {
                Name = name.Trim(),
                TaxCode = taxCode?.Trim(),
                AddressStreet = addressStreet?.Trim(),
                AddressWard = addressWard?.Trim(),
                AddressDistrict = addressDistrict?.Trim(),
                AddressCity = addressCity?.Trim(),
                ContactName = contactName?.Trim(),
                Phone = phone?.Trim(),
                Email = email?.Trim().ToLower(),
                IsActive = true
            };
        }

        public void Update(string name, string? contactName, string? phone, string? email)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Tên nhà cung cấp không được trống");

            Name = name.Trim();
            ContactName = contactName?.Trim();
            Phone = phone?.Trim();
            Email = email?.Trim().ToLower();
        }

        public void UpdateBankInfo(string bankAccount, string bankName)
        {
            BankAccount = bankAccount?.Trim();
            BankName = bankName?.Trim();
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}
