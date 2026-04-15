using Application.Interfaces.Repositories;
using Domain.Entities.Catalog;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ProductCommentRepository : IProductCommentRepository
    {
        private readonly AppDbContext _context;

        public ProductCommentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ProductComment?> GetByIdAsync(int id)
        {
            return await _context.ProductComments
                .AsNoTracking()
                .Include(c => c.Product)
                .Include(c => c.Replies)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<ProductComment?> GetByIdForUpdateAsync(int id)
        {
            return await _context.ProductComments
                .Include(c => c.Replies)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<ProductComment>> GetAllAsync()
        {
            return await _context.ProductComments
                .AsNoTracking()
                .Include(c => c.Product)
                .Include(c => c.Replies)
                .Where(c => c.ParentCommentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<ProductComment>> GetByProductAsync(int productId)
        {
            return await _context.ProductComments
                .AsNoTracking()
                .Include(c => c.Product)
                .Include(c => c.Replies)
                .Where(c => c.ProductId == productId && c.ParentCommentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<ProductComment>> GetByUserAsync(int userId)
        {
            return await _context.ProductComments
                .AsNoTracking()
                .Include(c => c.Product)
                .Include(c => c.Replies)
                .Where(c => c.UserId == userId && c.ParentCommentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<ProductComment>> GetByOrderAsync(int orderId)
        {
            return await _context.ProductComments
                .AsNoTracking()
                .Include(c => c.Product)
                .Include(c => c.Replies)
                .Where(c => c.OrderId == orderId && c.ParentCommentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<ProductComment?> GetByProductAndOrderAsync(int productId, int orderId)
        {
            return await _context.ProductComments
                .AsNoTracking()
                .Include(c => c.Product)
                .Include(c => c.Replies)
                .FirstOrDefaultAsync(c => c.ProductId == productId && c.OrderId == orderId && c.ParentCommentId == null);
        }

        public async Task<List<ProductComment>> GetPendingApprovalAsync()
        {
            return await _context.ProductComments
                .AsNoTracking()
                .Include(c => c.Product)
                .Include(c => c.Replies)
                .Where(c => !c.IsApproved && c.ParentCommentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _context.ProductComments.CountAsync();
        }

        public async Task<int> CountPendingAsync()
        {
            return await _context.ProductComments
                .CountAsync(c => !c.IsApproved);
        }

        public async Task AddAsync(ProductComment comment)
        {
            await _context.ProductComments.AddAsync(comment);
        }

        public void Update(ProductComment comment)
        {
            _context.ProductComments.Update(comment);
        }

        public void Delete(ProductComment comment)
        {
            _context.ProductComments.Remove(comment);
        }

        public async Task DeleteByIdAsync(int id)
        {
            var comment = await _context.ProductComments.FindAsync(id);
            if (comment != null)
            {
                _context.ProductComments.Remove(comment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
