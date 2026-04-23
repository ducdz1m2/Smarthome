namespace Domain.Enums
{
    public enum InstallationStatus
    {
        Pending = 0,              // Chờ xử lý (legacy)
        Assigned = 1,             // Đã phân công KT, chờ KT xác nhận
        Confirmed = 2,            // Đã xác nhận lịch
        TechnicianAssigned = 3,   // Đã có KT phụ trách (legacy)
        Preparing = 4,            // KT chuẩn bị đồ nghề + SP
        OnTheWay = 5,             // KT đang đi tới nhà khách
        Installing = 6,           // Đang lắp
        Testing = 7,              // Kiểm tra sau lắp
        Completed = 8,            // Hoàn thành, khách ký nhận
        Failed = 9,               // Lắp thất bại (cần quay lại)
        Rescheduled = 10,          // Đổi lịch
        Cancelled = 11,           // Hủy
        AwaitingMaterial = 12     // Chờ vật tư (hết sản phẩm thay thế trong khu vực)
    }
}
