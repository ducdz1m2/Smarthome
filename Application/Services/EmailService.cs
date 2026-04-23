using Application.Interfaces.Services;
using FluentEmail.Core;
using Microsoft.Extensions.Configuration;

namespace Application.Services;

public class EmailService : IEmailService
{
    private readonly IFluentEmail _fluentEmail;
    private readonly IConfiguration _configuration;

    public EmailService(IFluentEmailFactory fluentEmailFactory, IConfiguration configuration)
    {
        _fluentEmail = fluentEmailFactory.Create();
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
    {
        var fromEmail = _configuration["SmtpSettings:FromEmail"];
        var fromName = _configuration["SmtpSettings:FromName"];

        await _fluentEmail
            .To(toEmail)
            .Subject(subject)
            .Body(htmlContent, isHtml: true)
            .SendAsync();
    }

    public async Task SendTechnicianChangedEmailAsync(string toEmail, string orderNumber, string technicianName)
    {
        var subject = $"Thông báo thay đổi kỹ thuật viên - Đơn hàng #{orderNumber}";
        var htmlContent = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; background-color: #f9fafb; padding: 20px; border-radius: 8px;'>
                <div style='background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); padding: 30px; border-radius: 8px 8px 0 0; text-align: center;'>
                    <h1 style='color: white; margin: 0; font-size: 24px;'>Thông báo thay đổi kỹ thuật viên</h1>
                </div>
                <div style='background-color: white; padding: 30px; border-radius: 0 0 8px 8px;'>
                    <p style='color: #374151; font-size: 16px; margin: 0 0 20px 0;'>Chào bạn,</p>
                    <p style='color: #374151; font-size: 16px; margin: 0 0 20px 0;'>Đơn hàng <strong>#{orderNumber}</strong> đã được bàn giao cho kỹ thuật viên mới.</p>
                    <div style='background-color: #fef3c7; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #f59e0b;'>
                        <p style='color: #374151; font-size: 14px; margin: 0 0 10px 0; font-weight: bold;'>Kỹ thuật viên mới:</p>
                        <p style='color: #1f2937; font-size: 18px; margin: 0; font-weight: bold;'>{technicianName}</p>
                    </div>
                    <p style='color: #6b7280; font-size: 14px; margin: 20px 0 0 0;'>Vui lòng kiểm tra thông tin chi tiết trong tài khoản của bạn.</p>
                    <div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e5e7eb;'>
                        <p style='color: #9ca3af; font-size: 12px; margin: 0;'>Trân trọng,<br/>Đội ngũ Smarthome</p>
                    </div>
                </div>
            </div>";

        await SendEmailAsync(toEmail, subject, htmlContent);
    }

    public async Task SendRescheduleEmailAsync(string toEmail, string orderNumber, DateTime newDate)
    {
        var subject = $"Thông báo đổi lịch lắp đặt - Đơn hàng #{orderNumber}";
        var htmlContent = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; background-color: #f9fafb; padding: 20px; border-radius: 8px;'>
                <div style='background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%); padding: 30px; border-radius: 8px 8px 0 0; text-align: center;'>
                    <h1 style='color: white; margin: 0; font-size: 24px;'>Thông báo đổi lịch lắp đặt</h1>
                </div>
                <div style='background-color: white; padding: 30px; border-radius: 0 0 8px 8px;'>
                    <p style='color: #374151; font-size: 16px; margin: 0 0 20px 0;'>Chào bạn,</p>
                    <p style='color: #374151; font-size: 16px; margin: 0 0 20px 0;'>Lịch lắp đặt cho đơn hàng <strong>#{orderNumber}</strong> đã được thay đổi.</p>
                    <div style='background-color: #dbeafe; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #3b82f6;'>
                        <p style='color: #374151; font-size: 14px; margin: 0 0 10px 0; font-weight: bold;'>Ngày mới:</p>
                        <p style='color: #1f2937; font-size: 18px; margin: 0; font-weight: bold;'>{newDate:dd/MM/yyyy}</p>
                    </div>
                    <p style='color: #6b7280; font-size: 14px; margin: 20px 0 0 0;'>Vui lòng kiểm tra thông tin chi tiết trong tài khoản của bạn.</p>
                    <div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e5e7eb;'>
                        <p style='color: #9ca3af; font-size: 12px; margin: 0;'>Trân trọng,<br/>Đội ngũ Smarthome</p>
                    </div>
                </div>
            </div>";

        await SendEmailAsync(toEmail, subject, htmlContent);
    }

    public async Task SendOrderConfirmationEmailAsync(string toEmail, string orderNumber)
    {
        var subject = $"Xác nhận đơn hàng - #{orderNumber}";
        var htmlContent = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; background-color: #f9fafb; padding: 20px; border-radius: 8px;'>
                <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; border-radius: 8px 8px 0 0; text-align: center;'>
                    <h1 style='color: white; margin: 0; font-size: 24px;'>Xác nhận đơn hàng</h1>
                </div>
                <div style='background-color: white; padding: 30px; border-radius: 0 0 8px 8px;'>
                    <p style='color: #374151; font-size: 16px; margin: 0 0 20px 0;'>Chào bạn,</p>
                    <p style='color: #374151; font-size: 16px; margin: 0 0 20px 0;'>Đơn hàng <strong>#{orderNumber}</strong> của bạn đã được xác nhận.</p>
                    <div style='background-color: #d1fae5; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #10b981;'>
                        <p style='color: #065f46; font-size: 16px; margin: 0;'>✅ Đơn hàng đã được xác nhận thành công!</p>
                    </div>
                    <p style='color: #6b7280; font-size: 14px; margin: 20px 0 0 0;'>Chúng tôi sẽ xử lý đơn hàng của bạn trong thời gian sớm nhất.</p>
                    <div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e5e7eb;'>
                        <p style='color: #9ca3af; font-size: 12px; margin: 0;'>Trân trọng,<br/>Đội ngũ Smarthome</p>
                    </div>
                </div>
            </div>";

        await SendEmailAsync(toEmail, subject, htmlContent);
    }

    public async Task SendInstallationCompletedEmailAsync(string toEmail, string orderNumber)
    {
        var subject = $"Hoàn thành lắp đặt - Đơn hàng #{orderNumber}";
        var htmlContent = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; background-color: #f9fafb; padding: 20px; border-radius: 8px;'>
                <div style='background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%); padding: 30px; border-radius: 8px 8px 0 0; text-align: center;'>
                    <h1 style='color: white; margin: 0; font-size: 24px;'>Hoàn thành lắp đặt</h1>
                </div>
                <div style='background-color: white; padding: 30px; border-radius: 0 0 8px 8px;'>
                    <p style='color: #374151; font-size: 16px; margin: 0 0 20px 0;'>Chào bạn,</p>
                    <p style='color: #374151; font-size: 16px; margin: 0 0 20px 0;'>Lịch lắp đặt cho đơn hàng <strong>#{orderNumber}</strong> đã hoàn thành.</p>
                    <div style='background-color: #d1fae5; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #10b981;'>
                        <p style='color: #065f46; font-size: 16px; margin: 0;'>✅ Lắp đặt đã hoàn thành thành công!</p>
                    </div>
                    <p style='color: #6b7280; font-size: 14px; margin: 20px 0 0 0;'>Cảm ơn bạn đã sử dụng dịch vụ của Smarthome.</p>
                    <div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e5e7eb;'>
                        <p style='color: #9ca3af; font-size: 12px; margin: 0;'>Trân trọng,<br/>Đội ngũ Smarthome</p>
                    </div>
                </div>
            </div>";

        await SendEmailAsync(toEmail, subject, htmlContent);
    }

    public async Task SendWarrantyApprovedEmailAsync(string toEmail, string orderNumber, int warrantyRequestId)
    {
        var subject = $"Yêu cầu bảo hành đã được duyệt - Đơn hàng #{orderNumber}";
        var htmlContent = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; background-color: #f9fafb; padding: 20px; border-radius: 8px;'>
                <div style='background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%); padding: 30px; border-radius: 8px 8px 0 0; text-align: center;'>
                    <h1 style='color: white; margin: 0; font-size: 24px;'>Yêu cầu bảo hành đã được duyệt</h1>
                </div>
                <div style='background-color: white; padding: 30px; border-radius: 0 0 8px 8px;'>
                    <p style='color: #374151; font-size: 16px; margin: 0 0 20px 0;'>Chào bạn,</p>
                    <p style='color: #374151; font-size: 16px; margin: 0 0 20px 0;'>Yêu cầu bảo hành <strong>#{warrantyRequestId}</strong> cho đơn hàng <strong>#{orderNumber}</strong> đã được duyệt.</p>
                    <div style='background-color: #d1fae5; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #10b981;'>
                        <p style='color: #065f46; font-size: 16px; margin: 0;'>✅ Yêu cầu bảo hành đã được duyệt thành công!</p>
                    </div>
                    <p style='color: #6b7280; font-size: 14px; margin: 20px 0 0 0;'>Kỹ thuật viên sẽ liên hệ với bạn để sắp xếp lịch bảo hành.</p>
                    <div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e5e7eb;'>
                        <p style='color: #9ca3af; font-size: 12px; margin: 0;'>Trân trọng,<br/>Đội ngũ Smarthome</p>
                    </div>
                </div>
            </div>";

        await SendEmailAsync(toEmail, subject, htmlContent);
    }

    public async Task SendWarrantyRejectedEmailAsync(string toEmail, string orderNumber, int warrantyRequestId, string reason)
    {
        var subject = $"Yêu cầu bảo hành đã bị từ chối - Đơn hàng #{orderNumber}";
        var htmlContent = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; background-color: #f9fafb; padding: 20px; border-radius: 8px;'>
                <div style='background: linear-gradient(135deg, #eb3349 0%, #f45c43 100%); padding: 30px; border-radius: 8px 8px 0 0; text-align: center;'>
                    <h1 style='color: white; margin: 0; font-size: 24px;'>Yêu cầu bảo hành đã bị từ chối</h1>
                </div>
                <div style='background-color: white; padding: 30px; border-radius: 0 0 8px 8px;'>
                    <p style='color: #374151; font-size: 16px; margin: 0 0 20px 0;'>Chào bạn,</p>
                    <p style='color: #374151; font-size: 16px; margin: 0 0 20px 0;'>Yêu cầu bảo hành <strong>#{warrantyRequestId}</strong> cho đơn hàng <strong>#{orderNumber}</strong> đã bị từ chối.</p>
                    <div style='background-color: #fee2e2; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #ef4444;'>
                        <p style='color: #374151; font-size: 14px; margin: 0 0 10px 0; font-weight: bold;'>Lý do từ chối:</p>
                        <p style='color: #991b1b; font-size: 16px; margin: 0;'>{reason}</p>
                    </div>
                    <p style='color: #6b7280; font-size: 14px; margin: 20px 0 0 0;'>Vui lòng liên hệ với chúng tôi nếu bạn có thắc mắc.</p>
                    <div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e5e7eb;'>
                        <p style='color: #9ca3af; font-size: 12px; margin: 0;'>Trân trọng,<br/>Đội ngũ Smarthome</p>
                    </div>
                </div>
            </div>";

        await SendEmailAsync(toEmail, subject, htmlContent);
    }

    public async Task SendWarrantyCompletedEmailAsync(string toEmail, string orderNumber, int warrantyRequestId)
    {
        var subject = $"Bảo hành hoàn thành - Đơn hàng #{orderNumber}";
        var htmlContent = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; background-color: #f9fafb; padding: 20px; border-radius: 8px;'>
                <div style='background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%); padding: 30px; border-radius: 8px 8px 0 0; text-align: center;'>
                    <h1 style='color: white; margin: 0; font-size: 24px;'>Bảo hành hoàn thành</h1>
                </div>
                <div style='background-color: white; padding: 30px; border-radius: 0 0 8px 8px;'>
                    <p style='color: #374151; font-size: 16px; margin: 0 0 20px 0;'>Chào bạn,</p>
                    <p style='color: #374151; font-size: 16px; margin: 0 0 20px 0;'>Yêu cầu bảo hành <strong>#{warrantyRequestId}</strong> cho đơn hàng <strong>#{orderNumber}</strong> đã hoàn thành.</p>
                    <div style='background-color: #d1fae5; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #10b981;'>
                        <p style='color: #065f46; font-size: 16px; margin: 0;'>✅ Bảo hành đã hoàn thành thành công!</p>
                    </div>
                    <p style='color: #6b7280; font-size: 14px; margin: 20px 0 0 0;'>Cảm ơn bạn đã sử dụng dịch vụ của Smarthome.</p>
                    <div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e5e7eb;'>
                        <p style='color: #9ca3af; font-size: 12px; margin: 0;'>Trân trọng,<br/>Đội ngũ Smarthome</p>
                    </div>
                </div>
            </div>";

        await SendEmailAsync(toEmail, subject, htmlContent);
    }

    public async Task SendOrderStatusChangedEmailAsync(string toEmail, string orderNumber, string status)
    {
        var statusText = status switch
        {
            "Confirmed" => "đã được xác nhận",
            "Shipping" => "đang được giao",
            "Delivered" => "đã giao thành công",
            "Cancelled" => "đã bị hủy",
            _ => status
        };

        var statusColor = status switch
        {
            "Confirmed" => "#10b981",
            "Shipping" => "#3b82f6",
            "Delivered" => "#10b981",
            "Cancelled" => "#ef4444",
            _ => "#6b7280"
        };

        var subject = $"Cập nhật trạng thái đơn hàng - #{orderNumber}";
        var htmlContent = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; background-color: #f9fafb; padding: 20px; border-radius: 8px;'>
                <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; border-radius: 8px 8px 0 0; text-align: center;'>
                    <h1 style='color: white; margin: 0; font-size: 24px;'>Cập nhật trạng thái đơn hàng</h1>
                </div>
                <div style='background-color: white; padding: 30px; border-radius: 0 0 8px 8px;'>
                    <p style='color: #374151; font-size: 16px; margin: 0 0 20px 0;'>Chào bạn,</p>
                    <p style='color: #374151; font-size: 16px; margin: 0 0 20px 0;'>Đơn hàng của bạn có trạng thái mới:</p>
                    <div style='background-color: #f3f4f6; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid {statusColor};'>
                        <p style='color: #374151; font-size: 14px; margin: 0 0 10px 0; font-weight: bold;'>Mã đơn hàng:</p>
                        <p style='color: #1f2937; font-size: 20px; margin: 0 0 10px 0; font-weight: bold;'>#{orderNumber}</p>
                        <p style='color: #374151; font-size: 14px; margin: 0 0 10px 0; font-weight: bold;'>Trạng thái:</p>
                        <p style='color: {statusColor}; font-size: 18px; margin: 0; font-weight: bold;'>{statusText}</p>
                    </div>
                    <p style='color: #6b7280; font-size: 14px; margin: 20px 0 0 0;'>Vui lòng kiểm tra thông tin chi tiết trong tài khoản của bạn.</p>
                    <div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e5e7eb;'>
                        <p style='color: #9ca3af; font-size: 12px; margin: 0;'>Trân trọng,<br/>Đội ngũ Smarthome</p>
                    </div>
                </div>
            </div>";

        await SendEmailAsync(toEmail, subject, htmlContent);
    }

    public async Task SendInstallationAssignedEmailAsync(string toEmail, string orderNumber, string technicianName)
    {
        var subject = $"Phân công kỹ thuật viên - Đơn hàng #{orderNumber}";
        var htmlContent = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; background-color: #f9fafb; padding: 20px; border-radius: 8px;'>
                <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; border-radius: 8px 8px 0 0; text-align: center;'>
                    <h1 style='color: white; margin: 0; font-size: 24px;'>Phân công kỹ thuật viên</h1>
                </div>
                <div style='background-color: white; padding: 30px; border-radius: 0 0 8px 8px;'>
                    <p style='color: #374151; font-size: 16px; margin: 0 0 20px 0;'>Chào bạn,</p>
                    <p style='color: #374151; font-size: 16px; margin: 0 0 20px 0;'>Đơn hàng <strong>#{orderNumber}</strong> đã được phân công kỹ thuật viên.</p>
                    <div style='background-color: #fef3c7; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #f59e0b;'>
                        <p style='color: #374151; font-size: 14px; margin: 0 0 10px 0; font-weight: bold;'>Kỹ thuật viên:</p>
                        <p style='color: #1f2937; font-size: 18px; margin: 0; font-weight: bold;'>{technicianName}</p>
                    </div>
                    <p style='color: #6b7280; font-size: 14px; margin: 20px 0 0 0;'>Vui lòng kiểm tra thông tin chi tiết trong tài khoản của bạn.</p>
                    <div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e5e7eb;'>
                        <p style='color: #9ca3af; font-size: 12px; margin: 0;'>Trân trọng,<br/>Đội ngũ Smarthome</p>
                    </div>
                </div>
            </div>";

        await SendEmailAsync(toEmail, subject, htmlContent);
    }

    public async Task SendRatingApprovedEmailAsync(string toEmail, string type, int entityId)
    {
        var typeText = type switch
        {
            "ProductComment" => "đánh giá sản phẩm",
            "TechnicianRating" => "đánh giá kỹ thuật viên",
            _ => "đánh giá"
        };

        var subject = $"Đánh giá đã được duyệt";
        var htmlContent = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; background-color: #f9fafb; padding: 20px; border-radius: 8px;'>
                <div style='background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); padding: 30px; border-radius: 8px 8px 0 0; text-align: center;'>
                    <h1 style='color: white; margin: 0; font-size: 24px;'>Đánh giá đã được duyệt</h1>
                </div>
                <div style='background-color: white; padding: 30px; border-radius: 0 0 8px 8px;'>
                    <p style='color: #374151; font-size: 16px; margin: 0 0 20px 0;'>Chào bạn,</p>
                    <p style='color: #374151; font-size: 16px; margin: 0 0 20px 0;'>{typeText} của bạn đã được duyệt và hiển thị công khai.</p>
                    <div style='background-color: #d1fae5; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #10b981;'>
                        <p style='color: #065f46; font-size: 16px; margin: 0;'>✅ Đánh giá đã được duyệt thành công!</p>
                    </div>
                    <p style='color: #6b7280; font-size: 14px; margin: 20px 0 0 0;'>Cảm ơn bạn đã đóng góp ý kiến cho Smarthome.</p>
                    <div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e5e7eb;'>
                        <p style='color: #9ca3af; font-size: 12px; margin: 0;'>Trân trọng,<br/>Đội ngũ Smarthome</p>
                    </div>
                </div>
            </div>";

        await SendEmailAsync(toEmail, subject, htmlContent);
    }

    public async Task SendMaterialReadyEmailAsync(string toEmail, int bookingId, DateTime scheduledDate)
    {
        var subject = $"Vật tư đã sẵn sàng - Lịch lắp đặt #{bookingId}";
        var htmlContent = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; background-color: #f9fafb; padding: 20px; border-radius: 8px;'>
                <div style='background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%); padding: 30px; border-radius: 8px 8px 0 0; text-align: center;'>
                    <h1 style='color: white; margin: 0; font-size: 24px;'>Vật tư đã sẵn sàng</h1>
                </div>
                <div style='background-color: white; padding: 30px; border-radius: 0 0 8px 8px;'>
                    <p style='color: #374151; font-size: 16px; margin: 0 0 20px 0;'>Chào bạn,</p>
                    <p style='color: #374151; font-size: 16px; margin: 0 0 20px 0;'>Vật tư cho lịch lắp đặt <strong>#{bookingId}</strong> đã được điều phối và sẵn sàng.</p>
                    <div style='background-color: #dbeafe; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #3b82f6;'>
                        <p style='color: #374151; font-size: 14px; margin: 0 0 10px 0; font-weight: bold;'>Ngày thực hiện:</p>
                        <p style='color: #1f2937; font-size: 18px; margin: 0; font-weight: bold;'>{scheduledDate:dd/MM/yyyy}</p>
                    </div>
                    <p style='color: #6b7280; font-size: 14px; margin: 20px 0 0 0;'>Vui lòng kiểm tra thông tin chi tiết và xác nhận lịch trong tài khoản của bạn.</p>
                    <div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e5e7eb;'>
                        <p style='color: #9ca3af; font-size: 12px; margin: 0;'>Trân trọng,<br/>Đội ngũ Smarthome</p>
                    </div>
                </div>
            </div>";

        await SendEmailAsync(toEmail, subject, htmlContent);
    }
}
