using FIAP.CloudGames.Users.Domain.Entities;

namespace FIAP.CloudGames.Users.Domain.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task AddAsync(User user);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> UpdateAsync(User user);
        Task DeleteAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
    }
}
