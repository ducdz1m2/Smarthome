namespace Application.Interfaces.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlContent);
    Task SendTechnicianChangedEmailAsync(string toEmail, string orderNumber, string technicianName);
    Task SendRescheduleEmailAsync(string toEmail, string orderNumber, DateTime newDate);
    Task SendOrderConfirmationEmailAsync(string toEmail, string orderNumber);
    Task SendInstallationCompletedEmailAsync(string toEmail, string orderNumber);
    Task SendWarrantyApprovedEmailAsync(string toEmail, string orderNumber, int warrantyRequestId);
    Task SendWarrantyRejectedEmailAsync(string toEmail, string orderNumber, int warrantyRequestId, string reason);
    Task SendWarrantyCompletedEmailAsync(string toEmail, string orderNumber, int warrantyRequestId);
    Task SendOrderStatusChangedEmailAsync(string toEmail, string orderNumber, string status);
    Task SendInstallationAssignedEmailAsync(string toEmail, string orderNumber, string technicianName);
    Task SendRatingApprovedEmailAsync(string toEmail, string type, int entityId);
    Task SendMaterialReadyEmailAsync(string toEmail, int bookingId, DateTime scheduledDate);
}
