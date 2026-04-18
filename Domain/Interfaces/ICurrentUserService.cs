namespace Domain.Interfaces;

/// <summary>
/// Service for accessing current authenticated user information
/// </summary>
public interface ICurrentUserService
{
    int? UserId { get; }
    string UserName { get; }
    string Email { get; }
    string FullName { get; }
    List<string> Roles { get; }
    int? TechnicianId { get; }
    bool IsAuthenticated { get; }
}
