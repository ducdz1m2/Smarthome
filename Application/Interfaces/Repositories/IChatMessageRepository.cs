using Domain.Entities.Communication;
using Domain.Enums;

namespace Application.Interfaces.Repositories;

public interface IChatMessageRepository
{
    Task<ChatMessage?> GetByIdAsync(int id);
    Task<List<ChatMessage>> GetByChatRoomIdAsync(int chatRoomId, int limit = 50);
    Task AddAsync(ChatMessage message);
    void Update(ChatMessage message);
    void Delete(ChatMessage message);
    Task SaveChangesAsync();
}
