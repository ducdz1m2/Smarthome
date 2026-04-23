namespace Domain.Enums
{
    /// <summary>
    /// Trạng thái xử lý sản phẩm hư hỏng
    /// </summary>
    public enum DamagedProductStatus
    {
        /// <summary>
        /// Mới được đánh dấu hư hỏng, chưa xử lý
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Đang được sửa chữa
        /// </summary>
        UnderRepair = 1,

        /// <summary>
        /// Đã sửa xong, có thể đưa vào kho bán
        /// </summary>
        Repaired = 2,

        /// <summary>
        /// Không thể sửa, cần thay thế mới
        /// </summary>
        NeedsReplacement = 3,

        /// <summary>
        /// Đã hủy bỏ (ví dụ: sản phẩm không thể sử dụng, loại bỏ)
        /// </summary>
        Disposed = 4
    }
}
