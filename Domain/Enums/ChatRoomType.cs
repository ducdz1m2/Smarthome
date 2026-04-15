namespace Domain.Enums;

public enum ChatRoomType
{
    OneToOne = 0,      // Chat 1-1 giữa 2 người
    Support = 1,       // Chat hỗ trợ khách hàng (có thể có nhiều người)
    Group = 2          // Chat nhóm (nhiều người)
}
