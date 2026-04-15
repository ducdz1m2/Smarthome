namespace Application.DTOs.Requests;

public class CreateShipmentRequest
{
    public int OrderId { get; set; }
    public string Carrier { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class UpdateTrackingRequest
{
    public string TrackingNumber { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
