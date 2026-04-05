namespace Domain.Enums
{
    public enum InstallationStatus
    {
        Pending = 0,              // Chờ xác nhận lịch
        Confirmed = 1,            // Đã xác nhận lịch
        TechnicianAssigned = 2,   // Đã có KT phụ trách
        Preparing = 3,            // KT chuẩn bị đồ nghề + SP
        OnTheWay = 4,             // KT đang đi tới nhà khách
        Installing = 5,           // Đang lắp
        Testing = 6,              // Kiểm tra sau lắp
        Completed = 7,            // Hoàn thành, khách ký nhận
        Failed = 8,               // Lắp thất bại (cần quay lại)
        Rescheduled = 9,          // Đổi lịch
        Cancelled = 10            // Hủy
    }
}
