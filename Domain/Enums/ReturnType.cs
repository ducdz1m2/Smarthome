namespace Domain.Enums
{
    public enum ReturnType
    {
        Exchange = 0,             // Đổi sản phẩm khác
        Refund = 1,              // Hoàn tiền
        Repair = 2,              // Sửa chữa
    }

    public enum ReturnMethod
    {
        Shipping = 0,            // Trả qua bên vận chuyển
        Technician = 1           // Trả qua kỹ thuật viên (tháo lắp)
    }
}
