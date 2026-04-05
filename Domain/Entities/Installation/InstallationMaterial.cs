namespace Domain.Entities.Installation
{
    using Domain.Entities.Common;
    using Domain.Exceptions;

    public class InstallationMaterial : BaseEntity
    {
        public int BookingId { get; private set; }
        public int ProductId { get; private set; }
        public int QuantityTaken { get; private set; }
        public int? QuantityUsed { get; private set; }
        public int? QuantityReturned { get; private set; }

        public virtual InstallationBooking? Booking { get; private set; }

        private InstallationMaterial() { }

        public static InstallationMaterial Create(int bookingId, int productId, int quantityTaken)
        {
            if (quantityTaken <= 0)
                throw new DomainException("Số lượng lấy phải lớn hơn 0");

            return new InstallationMaterial
            {
                BookingId = bookingId,
                ProductId = productId,
                QuantityTaken = quantityTaken,
                QuantityUsed = null,
                QuantityReturned = null
            };
        }

        public void RecordUsage(int used)
        {
            if (used < 0 || used > QuantityTaken)
                throw new DomainException("Số lượng sử dụng không hợp lệ");

            QuantityUsed = used;
            QuantityReturned = QuantityTaken - used;
        }

        public void RecordReturn(int returned)
        {
            if (returned < 0 || returned > QuantityTaken)
                throw new DomainException("Số lượng trả không hợp lệ");

            QuantityReturned = returned;
            QuantityUsed = QuantityTaken - returned;
        }
    }
}
