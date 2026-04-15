using Application.Interfaces.Repositories;
using Domain.Entities.Communication;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Notification?> GetByIdAsync(int id)
        => await _context.Notifications.FindAsync(id);

    public async Task<List<Notification>> GetAllAsync()
        => await _context.Notifications.ToListAsync();

    public async Task<List<Notification>> GetByUserIdAsync(int userId, UserType userType)
        => await _context.Notifications
            .Where(n => n.UserId == userId && n.UserType == userType)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

    public async Task<List<Notification>> GetUnreadByUserIdAsync(int userId, UserType userType)
        => await _context.Notifications
            .Where(n => n.UserId == userId && n.UserType == userType && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

    public async Task<List<Notification>> GetByTypeAsync(NotificationType type)
        => await _context.Notifications
            .Where(n => n.Type == type)
            .ToListAsync();

    public async Task<int> CountUnreadAsync(int userId, UserType userType)
        => await _context.Notifications
            .CountAsync(n => n.UserId == userId && n.UserType == userType && !n.IsRead);

    public async Task<int> CountUnreadByTypeAsync(int userId, UserType userType, NotificationType type)
        => await _context.Notifications
            .CountAsync(n => n.UserId == userId && n.UserType == userType && n.Type == type && !n.IsRead);

    public async Task<List<Notification>> GetRecentAsync(int userId, UserType userType, int limit = 20)
        => await _context.Notifications
            .Where(n => n.UserId == userId && n.UserType == userType)
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync();

    public async Task<List<Notification>> GetUnsentAsync(int limit = 100)
        => await _context.Notifications
            .Where(n => !n.IsSent)
            .Take(limit)
            .ToListAsync();

    public async Task AddAsync(Notification notification)
        => await _context.Notifications.AddAsync(notification);

    public async Task AddRangeAsync(List<Notification> notifications)
        => await _context.Notifications.AddRangeAsync(notifications);

    public void Update(Notification notification)
        => _context.Notifications.Update(notification);

    public void UpdateRange(List<Notification> notifications)
        => _context.Notifications.UpdateRange(notifications);

    public void Delete(Notification notification)
        => _context.Notifications.Remove(notification);

    public async Task DeleteOldNotificationsAsync(int daysToKeep)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
        var oldNotifications = await _context.Notifications
            .Where(n => n.CreatedAt < cutoffDate && n.IsRead)
            .ToListAsync();
        _context.Notifications.RemoveRange(oldNotifications);
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
