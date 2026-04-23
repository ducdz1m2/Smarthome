using Domain.Interfaces;

namespace Web.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly object _lock = new();
    private int? _userId;
    private string _userName = "";
    private string _email = "";
    private string _fullName = "";
    private List<string> _roles = new();
    private int? _technicianId;

    public int? UserId
    {
        get
        {
            lock (_lock)
            {
                return _userId;
            }
        }
        private set
        {
            lock (_lock)
            {
                _userId = value;
            }
        }
    }

    public string UserName
    {
        get
        {
            lock (_lock)
            {
                return _userName;
            }
        }
        private set
        {
            lock (_lock)
            {
                _userName = value;
            }
        }
    }

    public string Email
    {
        get
        {
            lock (_lock)
            {
                return _email;
            }
        }
        private set
        {
            lock (_lock)
            {
                _email = value;
            }
        }
    }

    public string FullName
    {
        get
        {
            lock (_lock)
            {
                return _fullName;
            }
        }
        private set
        {
            lock (_lock)
            {
                _fullName = value;
            }
        }
    }

    public List<string> Roles
    {
        get
        {
            lock (_lock)
            {
                return new List<string>(_roles);
            }
        }
        private set
        {
            lock (_lock)
            {
                _roles = value.ToList();
            }
        }
    }

    public int? TechnicianId
    {
        get
        {
            lock (_lock)
            {
                return _technicianId;
            }
        }
        set
        {
            lock (_lock)
            {
                _technicianId = value;
            }
        }
    }

    public void SetUser(int userId, string userName, string email, string fullName, IEnumerable<string> roles, int? technicianId = null)
    {
        lock (_lock)
        {
            _userId = userId;
            _userName = userName;
            _email = email;
            _fullName = fullName;
            _roles = roles.ToList();
            _technicianId = technicianId;
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _userId = null;
            _userName = "";
            _email = "";
            _fullName = "";
            _roles = new();
            _technicianId = null;
        }
    }

    public bool IsAuthenticated => UserId.HasValue;
}
