namespace Domain.Entities.Content;

using Domain.Abstractions;
using Domain.Exceptions;
using Domain.ValueObjects;

/// <summary>
/// UserAddress aggregate root - represents a user's saved address.
/// </summary>
public class UserAddress : AggregateRoot
{
    public int UserId { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public string ReceiverName { get; private set; } = string.Empty;
    public PhoneNumber ReceiverPhone { get; private set; } = null!;
    public Address Address { get; private set; } = null!;
    public bool IsDefault { get; private set; } = false;

        private UserAddress() { }

        public static UserAddress Create(int userId, string label, string receiverName, PhoneNumber receiverPhone, Address address, bool isDefault = false)
        {
            if (string.IsNullOrWhiteSpace(label))
                throw new ValidationException(nameof(label), "Nhãn địa chỉ không được trống");

            if (string.IsNullOrWhiteSpace(receiverName))
                throw new ValidationException(nameof(receiverName), "Tên người nhận không được trống");

            return new UserAddress
            {
                UserId = userId,
                Label = label.Trim(),
                ReceiverName = receiverName.Trim(),
                ReceiverPhone = receiverPhone,
                Address = address,
                IsDefault = isDefault
            };
        }

        // Legacy overload for backward compatibility
        public static UserAddress Create(int userId, string label, string receiverName, string receiverPhone, string street, string? ward, string? district, string? city, bool isDefault = false)
        {
            var phone = PhoneNumber.Create(receiverPhone);
            var address = Address.Create(street, ward, district ?? "", city ?? "");
            return Create(userId, label, receiverName, phone, address, isDefault);
        }

        public void Update(string label, string receiverName, PhoneNumber receiverPhone, Address address)
        {
            if (string.IsNullOrWhiteSpace(label))
                throw new ValidationException(nameof(label), "Nhãn địa chỉ không được trống");

            if (string.IsNullOrWhiteSpace(receiverName))
                throw new ValidationException(nameof(receiverName), "Tên người nhận không được trống");

            Label = label.Trim();
            ReceiverName = receiverName.Trim();
            ReceiverPhone = receiverPhone;
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
