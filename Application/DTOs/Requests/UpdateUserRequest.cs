namespace Application.DTOs.Requests
{
    public class UpdateUserRequest
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Avatar { get; set; }
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class UpdateUserPasswordRequest
    {
        public int UserId { get; set; }
        public string NewPassword { get; set; } = string.Empty;
    }
}
