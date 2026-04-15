using Application.Interfaces.Repositories;
using Domain.Entities.Communication;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ChatMessageRepository : IChatMessageRepository
{
    private readonly AppDbContext _context;

    public ChatMessageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ChatMessage?> GetByIdAsync(int id)
        => await _context.ChatMessages
            .Include(m => m.Attachments)
            .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<List<ChatMessage>> GetByChatRoomIdAsync(int chatRoomId, int limit = 50)
        => await _context.ChatMessages
            .Where(m => m.ChatRoomId == chatRoomId)
            .OrderByDescending(m => m.SentAt)
            .Take(limit)
            .Include(m => m.Attachments)
            .ToListAsync();

    public async Task<List<ChatMessage>> GetByChatRoomIdPagedAsync(int chatRoomId, int page, int pageSize)
        => await _context.ChatMessages
            .Where(m => m.ChatRoomId == chatRoomId)
            .OrderByDescending(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(m => m.Attachments)
            .ToListAsync();

    public async Task<List<ChatMessage>> GetUnreadMessagesAsync(int chatRoomId, int userId)
        => await _context.ChatMessages
            .Where(m => m.ChatRoomId == chatRoomId && m.SenderId != userId)
            .ToListAsync();

    public async Task<int> CountUnreadMessagesAsync(int chatRoomId, int userId)
        => await _context.ChatMessages
            .Where(m => m.ChatRoomId == chatRoomId && m.SenderId != userId)
            .CountAsync();

    public async Task<ChatMessage?> GetLastMessageAsync(int chatRoomId)
        => await _context.ChatMessages
            .Where(m => m.ChatRoomId == chatRoomId)
            .OrderByDescending(m => m.SentAt)
            .FirstOrDefaultAsync();

    public async Task AddAsync(ChatMessage message)
        => await _context.ChatMessages.AddAsync(message);

    public void Update(ChatMessage message)
        => _context.ChatMessages.Update(message);

    public void Delete(ChatMessage message)
        => _context.ChatMessages.Remove(message);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
