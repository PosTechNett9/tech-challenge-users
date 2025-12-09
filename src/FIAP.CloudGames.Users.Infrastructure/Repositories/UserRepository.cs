using FIAP.CloudGames.Users.Domain.Entities;
using FIAP.CloudGames.Users.Domain.Interfaces.Repositories;
using FIAP.CloudGames.Users.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace FIAP.CloudGames.Users.Infrastructure.Repositories
{
    public class UserRepository(UsersDbContext context) : IUserRepository
    {
        private readonly UsersDbContext _context = context;

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Set<User>().ToListAsync();
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Set<User>().FindAsync(id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Set<User>().FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task AddAsync(User user)
        {
            await _context.Set<User>().AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> UpdateAsync(User user)
        {
            _context.Set<User>().Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await GetByIdAsync(id);
            if (user is null) return;

            _context.Set<User>().Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
