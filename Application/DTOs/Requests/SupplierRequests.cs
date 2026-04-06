namespace Application.DTOs.Requests
{
    public class CreateSupplierRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? TaxCode { get; set; }
        public string? AddressStreet { get; set; }
        public string? AddressWard { get; set; }
        public string? AddressDistrict { get; set; }
        public string? AddressCity { get; set; }
        public string? ContactName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? BankAccount { get; set; }
        public string? BankName { get; set; }
    }

    public class UpdateSupplierRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? ContactName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? BankAccount { get; set; }
        public string? BankName { get; set; }
        public bool IsActive { get; set; }
    }
}
