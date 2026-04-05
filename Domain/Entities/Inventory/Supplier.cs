namespace Domain.Entities.Inventory
{
    using Domain.Entities.Common;
    using Domain.Exceptions;
    using Domain.ValueObjects;

    public class Supplier : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string? TaxCode { get; private set; }
        public Address? Address { get; private set; }
        public string? ContactName { get; private set; }
        public PhoneNumber? Phone { get; private set; }
        public Email? Email { get; private set; }
        public string? BankAccount { get; private set; }
        public string? BankName { get; private set; }
        public bool IsActive { get; private set; } = true;

        public virtual ICollection<StockEntry> StockEntries { get; private set; } = new List<StockEntry>();

        private Supplier() { }

        public static Supplier Create(string name, string? taxCode = null, Address? address = null,
            string? contactName = null, PhoneNumber? phone = null, Email? email = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Tên nhà cung cấp không được trống");

            return new Supplier
            {
                Name = name.Trim(),
                TaxCode = taxCode?.Trim(),
                Address = address,
                ContactName = contactName?.Trim(),
                Phone = phone,
                Email = email,
                IsActive = true
            };
        }

        public void Update(string name, string? contactName, PhoneNumber? phone, Email? email)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Tên nhà cung cấp không được trống");

            Name = name.Trim();
            ContactName = contactName?.Trim();
            Phone = phone;
            Email = email;
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
