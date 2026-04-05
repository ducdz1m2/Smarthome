namespace Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 0,              // Mới tạo, chờ xác nhận
        Confirmed = 1,            // Đã xác nhận, đang xử lý
        AwaitingPickup = 10,      // Chờ shipper lấy hàng (SP giao)
        Shipping = 11,            // Đang giao
        Delivered = 12,           // Đã giao
        AwaitingSchedule = 20,    // Chờ đặt lịch lắp (SP lắp)
        Scheduled = 21,           // Đã có lịch lắp
        TechnicianAssigned = 22,  // Đã phân công KT
        Preparing = 23,           // KT chuẩn bị (lấy SP từ kho)
        Installing = 24,          // Đang lắp
        Testing = 25,             // Kiểm tra sau lắp
        Completed = 30,           // Hoàn tất
        Cancelled = 40,           // Đã hủy
        Refunded = 41,            // Đã hoàn tiền
        ReturnRequested = 42      // Yêu cầu trả hàng
    }
}
