namespace Application.DTOs.Requests
{
    public class CreateWarrantyClaimRequest
    {
        public int WarrantyId { get; set; }
        public string Issue { get; set; } = string.Empty;
    }
}
