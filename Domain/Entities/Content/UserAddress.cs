namespace Domain.Entities.Content
{
    using Domain.Entities.Common;
    using Domain.Exceptions;
    using Domain.ValueObjects;

    public class UserAddress : BaseEntity
    {
        public int UserId { get; private set; }
        public string Label { get; private set; } = string.Empty; // "Nhà riêng", "Công ty"
        public string ReceiverName { get; private set; } = string.Empty;
        public PhoneNumber ReceiverPhone { get; private set; } = null!;
        public Address Address { get; private set; } = null!;
        public bool IsDefault { get; private set; } = false;

        private UserAddress() { }

        public static UserAddress Create(int userId, string label, string receiverName, string receiverPhone, Address address, bool isDefault = false)
        {
            if (string.IsNullOrWhiteSpace(label))
                throw new DomainException("Nhãn địa chỉ không được trống");

            if (string.IsNullOrWhiteSpace(receiverName))
                throw new DomainException("Tên người nhận không được trống");

            return new UserAddress
            {
                UserId = userId,
                Label = label.Trim(),
                ReceiverName = receiverName.Trim(),
                ReceiverPhone = PhoneNumber.Create(receiverPhone),
                Address = address,
                IsDefault = isDefault
            };
        }

        public void Update(string label, string receiverName, string receiverPhone, Address address)
        {
            if (string.IsNullOrWhiteSpace(label))
                throw new DomainException("Nhãn địa chỉ không được trống");

            if (string.IsNullOrWhiteSpace(receiverName))
                throw new DomainException("Tên người nhận không được trống");

            Label = label.Trim();
            ReceiverName = receiverName.Trim();
            ReceiverPhone = PhoneNumber.Create(receiverPhone);
            Address = address;
        }

        public void SetAsDefault()
        {
            IsDefault = true;
        }

        public void UnsetDefault()
        {
            IsDefault = false;
        }
    }
}
