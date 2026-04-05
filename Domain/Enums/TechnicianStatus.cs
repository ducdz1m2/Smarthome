namespace Domain.Enums
{
    public enum TechnicianStatus
    {
        OffDuty = 0,              // Không làm việc
        Available = 1,            // Rảnh, có thể nhận job
        Assigned = 2,             // Đã có lịch hôm nay/tuần này
        OnTheWay = 3,             // Đang đi tới nhà khách
        Installing = 4,           // Đang lắp tại nhà khách
        Completed = 5,            // Hoàn thành job
        Break = 6                 // Nghỉ giữa ca
    }
}
