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
            .Include(m => m.Attachments)
            .Where(m => m.ChatRoomId == chatRoomId && !m.IsDeleted)
            .OrderByDescending(m => m.SentAt)
            .Take(limit)
            .ToListAsync();

    public async Task AddAsync(ChatMessage message)
        => await _context.ChatMessages.AddAsync(message);

    public void Update(ChatMessage message)
        => _context.ChatMessages.Update(message);

    public void Delete(ChatMessage message)
        => _context.ChatMessages.Remove(message);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
