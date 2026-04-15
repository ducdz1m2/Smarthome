namespace Web.Services;

public class CurrentUserService
{
    public int? UserId { get; private set; }
    public string? UserName { get; private set; }
    public string? Email { get; private set; }
    public string? FullName { get; private set; }
    public int? TechnicianId { get; private set; }
    public List<string> Roles { get; private set; } = new();

    public void SetUser(int userId, string userName, string email, string fullName, IEnumerable<string> roles, int? technicianId = null)
    {
        Console.WriteLine($"[CurrentUserService] SetUser called: UserId={userId}, UserName={userName}, TechnicianId={technicianId}");
        UserId = userId;
        UserName = userName;
        Email = email;
        FullName = fullName;
        TechnicianId = technicianId;
        Roles = roles.ToList();
        Console.WriteLine($"[CurrentUserService] IsAuthenticated after SetUser: {IsAuthenticated}");
    }

    public void Clear()
    {
        Console.WriteLine($"[CurrentUserService] Clear called");
        UserId = null;
        UserName = null;
        Email = null;
        FullName = null;
        TechnicianId = null;
        Roles = new List<string>();
    }

    public bool IsAuthenticated => UserId.HasValue;
}
