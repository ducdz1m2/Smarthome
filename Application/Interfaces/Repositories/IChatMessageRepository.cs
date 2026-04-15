using Domain.Entities.Communication;

namespace Application.Interfaces.Repositories;

public interface IChatMessageRepository
{
    Task<ChatMessage?> GetByIdAsync(int id);
    Task<List<ChatMessage>> GetByChatRoomIdAsync(int chatRoomId, int limit = 50);
    Task<List<ChatMessage>> GetByChatRoomIdPagedAsync(int chatRoomId, int page, int pageSize);
    Task<List<ChatMessage>> GetUnreadMessagesAsync(int chatRoomId, int userId);
    Task<int> CountUnreadMessagesAsync(int chatRoomId, int userId);
    Task<ChatMessage?> GetLastMessageAsync(int chatRoomId);
    Task AddAsync(ChatMessage message);
    void Update(ChatMessage message);
    void Delete(ChatMessage message);
    Task SaveChangesAsync();
}
