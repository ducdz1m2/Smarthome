using Domain.Entities.Content;

namespace Application.Interfaces.Repositories
{
    public interface IUserAddressRepository
    {
        Task<UserAddress?> GetByIdAsync(int id);
        Task<List<UserAddress>> GetByUserIdAsync(int userId);
        Task<UserAddress?> GetDefaultByUserIdAsync(int userId);
        Task AddAsync(UserAddress address);
        void Update(UserAddress address);
        void Delete(UserAddress address);
        Task SaveChangesAsync();
    }
}
