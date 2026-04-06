namespace Application.DTOs.Responses
{
    public class SupplierResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? TaxCode { get; set; }
        public string? Address { get; set; }
        public string? ContactName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? BankAccount { get; set; }
        public string? BankName { get; set; }
        public bool IsActive { get; set; }
        public int StockEntryCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
