namespace Application.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Avatar { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class CreateUserRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class UpdateUserRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Avatar { get; set; }
    }

    public class UserAddressDto
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
        public string? ReceiverPhone { get; set; }
        public string Street { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }

    public class CreateUserAddressRequest
    {
        public string Label { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
        public string? ReceiverPhone { get; set; }
        public string Street { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }
}
