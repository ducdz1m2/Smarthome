namespace Application.DTOs.Responses
{
    public class UserAddressResponse
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
        public string? ReceiverPhone { get; set; }
        public string Street { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }
}
