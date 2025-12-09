using FIAP.CloudGames.Users.Application.Dtos;
using FIAP.CloudGames.Users.Domain.Entities;

namespace FIAP.CloudGames.Users.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(Guid id);
        Task<User> CreateAsync(CreateUserDto dto);
        Task<User?> UpdateAsync(UpdateUserDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
