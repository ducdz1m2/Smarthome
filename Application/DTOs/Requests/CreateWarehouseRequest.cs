namespace Application.DTOs.Requests
{
    public class CreateWarehouseRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? AddressStreet { get; set; }
        public string? AddressWard { get; set; }
        public string? AddressDistrict { get; set; }
        public string? AddressCity { get; set; }
        public string? Phone { get; set; }
    }
}
