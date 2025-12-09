using FIAP.CloudGames.Users.Application.Dtos;
using FIAP.CloudGames.Users.Application.Interfaces;
using FIAP.CloudGames.Users.Domain.Interfaces;
using FIAP.CloudGames.Users.Domain.Interfaces.Repositories;

namespace FIAP.CloudGames.Users.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<string> LoginAsync(LoginUserDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);

            if (user == null || !user.Password.Verify(dto.Password))
                throw new UnauthorizedAccessException("Credenciais inválidas");

            return _jwtTokenGenerator.GenerateToken(user.Id.ToString(), user.Email, user.Role.ToString());
        }
    }
}
