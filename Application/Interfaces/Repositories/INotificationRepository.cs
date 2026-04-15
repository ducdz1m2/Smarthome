using Domain.Entities.Communication;
using Domain.Enums;

namespace Application.Interfaces.Repositories;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(int id);
    Task<List<Notification>> GetAllAsync();
    Task<List<Notification>> GetByUserIdAsync(int userId, UserType userType);
    Task<List<Notification>> GetUnreadByUserIdAsync(int userId, UserType userType);
    Task<List<Notification>> GetByTypeAsync(NotificationType type);
    Task<int> CountUnreadAsync(int userId, UserType userType);
    Task<int> CountUnreadByTypeAsync(int userId, UserType userType, NotificationType type);
    Task<List<Notification>> GetRecentAsync(int userId, UserType userType, int limit = 20);
    Task<List<Notification>> GetUnsentAsync(int limit = 100);
    Task AddAsync(Notification notification);
    Task AddRangeAsync(List<Notification> notifications);
    void Update(Notification notification);
    void UpdateRange(List<Notification> notifications);
    void Delete(Notification notification);
    Task DeleteOldNotificationsAsync(int daysToKeep);
    Task SaveChangesAsync();
}
