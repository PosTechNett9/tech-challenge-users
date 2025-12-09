using FIAP.CloudGames.Users.Application.Dtos;
using FIAP.CloudGames.Users.Application.Interfaces;
using FIAP.CloudGames.Users.Domain.Entities;
using FIAP.CloudGames.Users.Domain.Interfaces.Repositories;
using FIAP.CloudGames.Users.Domain.ValueObjects;

namespace FIAP.CloudGames.Users.Application.Services
{
    public class UserService(IUserRepository userRepository) : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository;

        public async Task<User> CreateAsync(CreateUserDto dto)
        {
            var password = Password.FromPlainText(dto.Password);
            var email = Email.Create(dto.Email);

            var user = new User(
                dto.Name,
                email,
                password,
                dto.Role
            );

            await _userRepository.AddAsync(user);
            return user;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<User?> UpdateAsync(UpdateUserDto dto)
        {
            var user = await _userRepository.GetByIdAsync(dto.Id);
            if (user == null) return null;

            if (dto.Email != null)
            {
                var email = Email.Create(dto.Email);
                user.Email = email;
            }

            if (dto.Name != null)
                user.Name = dto.Name;

            if (dto.Role != null)
                user.Role = dto.Role.Value;

            return await _userRepository.UpdateAsync(user);
        }

        public async Task<User?> UpdatePasswordAsync(UpdateUserPasswordDto dto)
        {
            var user = await _userRepository.GetByIdAsync(dto.Id);
            if (user == null) return null;

            var password = Password.FromPlainText(dto.Password);

            user.Password = password;

            return await _userRepository.UpdateAsync(user);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return false;

            await _userRepository.DeleteAsync(user.Id);
            return true;
        }
    }
}
