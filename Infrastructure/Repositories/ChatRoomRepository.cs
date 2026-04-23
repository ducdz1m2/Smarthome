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
            .Include(r => r.Messages.OrderByDescending(m => m.SentAt).Take(messageLimit))
                .ThenInclude(m => m.Attachments)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<List<ChatRoom>> GetAllAsync()
        => await _context.ChatRooms
            .Include(r => r.Participants)
            .ToListAsync();

    public async Task<List<ChatRoom>> GetByUserIdAsync(int userId, UserType userType)
    {
        try
        {
            // Simplified query to avoid connection closed error
            var chatRoomIds = await _context.ChatParticipants
                .Where(p => p.UserId == userId && p.UserType == userType && p.IsActive)
                .Select(p => p.ChatRoomId)
                .Distinct()
                .ToListAsync();

            if (!chatRoomIds.Any())
                return new List<ChatRoom>();

            var chatRooms = await _context.ChatRooms
                .Where(r => chatRoomIds.Contains(r.Id))
                .Include(r => r.Participants)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            // Load messages separately to avoid complex query
            foreach (var room in chatRooms)
            {
                _context.Entry(room).Collection(r => r.Messages).Query()
                    .OrderByDescending(m => m.SentAt)
                    .Take(1)
                    .Load();
            }

            return chatRooms;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatRoomRepository.GetByUserIdAsync] Error: {ex.Message}");
            return new List<ChatRoom>();
        }
    }

    public async Task<List<ChatRoom>> GetByParticipantAsync(int userId, UserType userType)
        => await GetByUserIdAsync(userId, userType);

    public async Task<ChatRoom?> GetOneToOneRoomAsync(int user1Id, UserType user1Type, int user2Id, UserType user2Type)
        => await _context.ChatRooms
            .Include(r => r.Participants)
            .Where(r => r.Type == ChatRoomType.OneToOne
                && r.Participants.Any(p => p.UserId == user1Id && p.UserType == user1Type)
                && r.Participants.Any(p => p.UserId == user2Id && p.UserType == user2Type))
            .FirstOrDefaultAsync();

    public async Task<List<ChatRoom>> GetActiveSupportRoomsAsync()
        => await _context.ChatRooms
            .Include(r => r.Participants)
            .Include(r => r.Messages.OrderByDescending(m => m.SentAt).Take(1))
            .Where(r => r.Type == ChatRoomType.Support && r.IsActive)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<List<ChatRoom>> GetUnassignedSupportRoomsAsync()
        => await _context.ChatRooms
            .Include(r => r.Participants)
            .Where(r => r.Type == ChatRoomType.Support && r.IsActive
                && !r.Participants.Any(p => p.UserType == UserType.Admin))
            .ToListAsync();

    public async Task<ChatRoom?> GetByInstallationIdAsync(int installationId)
        => await _context.ChatRooms
            .Include(r => r.Participants)
            .FirstOrDefaultAsync(r => r.RelatedInstallationId == installationId);

    public async Task<List<ChatRoom>> GetByTechnicianIdAsync(int technicianId)
        => await _context.ChatRooms
            .Include(r => r.Participants)
            .Include(r => r.Messages.OrderByDescending(m => m.SentAt).Take(1))
            .Where(r => r.Participants.Any(p => p.UserId == technicianId && p.UserType == UserType.Technician && p.IsActive))
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<bool> ExistsOneToOneAsync(int user1Id, UserType user1Type, int user2Id, UserType user2Type)
        => await _context.ChatRooms
            .AnyAsync(r => r.Type == ChatRoomType.OneToOne
                && r.Participants.Any(p => p.UserId == user1Id && p.UserType == user1Type)
                && r.Participants.Any(p => p.UserId == user2Id && p.UserType == user2Type));

    public async Task AddAsync(ChatRoom chatRoom)
        => await _context.ChatRooms.AddAsync(chatRoom);

    public void Update(ChatRoom chatRoom)
        => _context.ChatRooms.Update(chatRoom);

    public void Delete(ChatRoom chatRoom)
        => _context.ChatRooms.Remove(chatRoom);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
