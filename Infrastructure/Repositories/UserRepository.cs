using Domain.Entities.Identity;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<ApplicationUser?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _userManager.FindByIdAsync(id.ToString());
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<ApplicationUser?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default)
    {
        return await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone, cancellationToken);
    }

    public async Task<ApplicationUser?> GetByIdWithAddressesAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<ApplicationUser?> GetByIdWithOrdersAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _userManager.Users.Where(u => u.Email == email);
        if (excludeId.HasValue)
        {
            query = query.Where(u => u.Id != excludeId.Value);
        }
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> PhoneExistsAsync(string phone, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _userManager.Users.Where(u => u.PhoneNumber == phone);
        if (excludeId.HasValue)
        {
            query = query.Where(u => u.Id != excludeId.Value);
        }
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ApplicationUser>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _userManager.Users
            .Where(u => u.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ApplicationUser>> SearchAsync(string keyword, CancellationToken cancellationToken = default)
    {
        return await _userManager.Users
            .Where(u => (u.FullName != null && u.FullName.Contains(keyword)) || (u.Email != null && u.Email.Contains(keyword)))
            .ToListAsync(cancellationToken);
    }

    public async Task SaveAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    public async Task DeleteAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}
