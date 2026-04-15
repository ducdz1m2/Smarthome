using Application.Interfaces.Repositories;
using Domain.Entities.Communication;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ChatRoomRepository : IChatRoomRepository
{
    private readonly AppDbContext _context;

    public ChatRoomRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ChatRoom?> GetByIdAsync(int id)
        => await _context.ChatRooms.FindAsync(id);

    public async Task<ChatRoom?> GetByIdWithParticipantsAsync(int id)
        => await _context.ChatRooms
            .Include(r => r.Participants)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<ChatRoom?> GetByIdWithMessagesAsync(int id, int messageLimit = 50)
        => await _context.ChatRooms
            .Include(r => r.Participants)
            .Include(r => r.Messages)
                .ThenInclude(m => m.Attachments)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<List<ChatRoom>> GetAllAsync()
        => await _context.ChatRooms.ToListAsync();

    public async Task<List<ChatRoom>> GetByUserIdAsync(int userId, UserType userType)
        => await _context.ChatRooms
            .Where(r => r.Participants.Any(p => p.UserId == userId && p.UserType == userType))
            .ToListAsync();

    public async Task<List<ChatRoom>> GetByParticipantAsync(int userId, UserType userType)
        => await _context.ChatRooms
            .Where(r => r.Participants.Any(p => p.UserId == userId && p.UserType == userType))
            .ToListAsync();

    public async Task<ChatRoom?> GetOneToOneRoomAsync(int user1Id, UserType user1Type, int user2Id, UserType user2Type)
        => await _context.ChatRooms
            .Where(r => r.Type == ChatRoomType.OneToOne)
            .Where(r => r.Participants.Any(p => p.UserId == user1Id && p.UserType == user1Type))
            .Where(r => r.Participants.Any(p => p.UserId == user2Id && p.UserType == user2Type))
            .FirstOrDefaultAsync();

    public async Task<List<ChatRoom>> GetActiveSupportRoomsAsync()
        => await _context.ChatRooms
            .Where(r => r.Type == ChatRoomType.Support && r.IsActive)
            .ToListAsync();

    public async Task<List<ChatRoom>> GetUnassignedSupportRoomsAsync()
        => await _context.ChatRooms
            .Where(r => r.Type == ChatRoomType.Support && r.IsActive)
            .Where(r => !r.Participants.Any(p => p.UserType == UserType.Technician))
            .ToListAsync();

    public async Task<bool> ExistsOneToOneAsync(int user1Id, UserType user1Type, int user2Id, UserType user2Type)
        => await GetOneToOneRoomAsync(user1Id, user1Type, user2Id, user2Type) != null;

    public async Task AddAsync(ChatRoom chatRoom)
        => await _context.ChatRooms.AddAsync(chatRoom);

    public void Update(ChatRoom chatRoom)
        => _context.ChatRooms.Update(chatRoom);

    public void Delete(ChatRoom chatRoom)
        => _context.ChatRooms.Remove(chatRoom);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
