using Domain.Entities.Communication;
using Domain.Enums;

namespace Application.Interfaces.Repositories;

public interface IChatRoomRepository
{
    Task<ChatRoom?> GetByIdAsync(int id);
    Task<ChatRoom?> GetByIdWithParticipantsAsync(int id);
    Task<ChatRoom?> GetByIdWithMessagesAsync(int id, int messageLimit = 50);
    Task<List<ChatRoom>> GetAllAsync();
    Task<List<ChatRoom>> GetByUserIdAsync(int userId, UserType userType);
    Task<List<ChatRoom>> GetByParticipantAsync(int userId, UserType userType);
    Task<ChatRoom?> GetOneToOneRoomAsync(int user1Id, UserType user1Type, int user2Id, UserType user2Type);
    Task<List<ChatRoom>> GetActiveSupportRoomsAsync();
    Task<List<ChatRoom>> GetUnassignedSupportRoomsAsync();
    Task<bool> ExistsOneToOneAsync(int user1Id, UserType user1Type, int user2Id, UserType user2Type);
    Task AddAsync(ChatRoom chatRoom);
    void Update(ChatRoom chatRoom);
    void Delete(ChatRoom chatRoom);
    Task SaveChangesAsync();
}
