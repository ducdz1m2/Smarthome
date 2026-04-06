namespace Domain.Entities.Content
{
    using Domain.Entities.Common;
    using Domain.Exceptions;

    public class UserAddress : BaseEntity
    {
        public int UserId { get; private set; }
        public string Label { get; private set; } = string.Empty;
        public string ReceiverName { get; private set; } = string.Empty;
        public string ReceiverPhone { get; private set; } = null!;
        public string Street { get; private set; } = null!;
        public string? Ward { get; private set; }
        public string? District { get; private set; }
        public string? City { get; private set; }
        public bool IsDefault { get; private set; } = false;

        private UserAddress() { }

        public static UserAddress Create(int userId, string label, string receiverName, string receiverPhone, string street, string? ward, string? district, string? city, bool isDefault = false)
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
                ReceiverPhone = receiverPhone.Trim(),
                Street = street.Trim(),
                Ward = ward?.Trim(),
                District = district?.Trim(),
                City = city?.Trim(),
                IsDefault = isDefault
            };
        }

        public void Update(string label, string receiverName, string receiverPhone, string street, string? ward, string? district, string? city)
        {
            if (string.IsNullOrWhiteSpace(label))
                throw new DomainException("Nhãn địa chỉ không được trống");

            if (string.IsNullOrWhiteSpace(receiverName))
                throw new DomainException("Tên người nhận không được trống");

            Label = label.Trim();
            ReceiverName = receiverName.Trim();
            ReceiverPhone = receiverPhone.Trim();
            Street = street.Trim();
            Ward = ward?.Trim();
            District = district?.Trim();
            City = city?.Trim();
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
