using FIAP.CloudGames.Users.Application.Dtos;
using FIAP.CloudGames.Users.Application.Interfaces;
using FIAP.CloudGames.Users.Domain.Interfaces;
using FIAP.CloudGames.Users.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace FIAP.CloudGames.Users.Application.Services
{
    public class AuthService(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator, ILogger<AuthService> logger) : IAuthService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;
        private readonly ILogger<AuthService> _logger = logger;

        public async Task<string> LoginAsync(LoginUserDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);

            if (user == null || !user.Password.Verify(dto.Password))
                throw new UnauthorizedAccessException("Credenciais inválidas");

            return _jwtTokenGenerator.GenerateToken(user.Id.ToString(), user.Email, user.Role.ToString());
        }

        public async Task<AuthenticationResponseEvent> AuthenticateAsync(
            AuthenticationRequestEvent request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "[AUTH-SERVICE] Authenticating user: {Email}, RequestId={RequestId}",
                    request.Email,
                    request.RequestId);

                // Buscar usuário pelo username
                var user = await _userRepository.GetByEmailAsync(request.Email);

                if (user == null)
                {
                    _logger.LogWarning(
                        "[AUTH-SERVICE] User not found: {Email}, RequestId={RequestId}",
                        request.Email,
                        request.RequestId);

                    return new AuthenticationResponseEvent
                    {
                        RequestId = request.RequestId,
                        Success = false,
                        ErrorMessage = "Invalid credentials",
                        RespondedAt = DateTime.UtcNow
                    };
                }

                // Verificar senha
                if (!user.Password.Verify(request.Password))
                {
                    _logger.LogWarning(
                        "[AUTH-SERVICE] Invalid password for user: {Email}, RequestId={RequestId}",
                        request.Email,
                        request.RequestId);

                    return new AuthenticationResponseEvent
                    {
                        RequestId = request.RequestId,
                        Success = false,
                        ErrorMessage = "Invalid credentials",
                        RespondedAt = DateTime.UtcNow
                    };
                }

                // Gerar token JWT
                var token = _jwtTokenGenerator.GenerateToken(user.Id.ToString(), user.Email, user.Role.ToString());

                _logger.LogInformation(
                    "[AUTH-SERVICE] User authenticated successfully: {Email}, UserId={UserId}, RequestId={RequestId}",
                    request.Email,
                    user.Id,
                    request.RequestId);

                return new AuthenticationResponseEvent
                {
                    RequestId = request.RequestId,
                    Success = true,
                    UserId = user.Id,
                    Token = token,
                    RespondedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[AUTH-SERVICE] Error authenticating user: {Email}, RequestId={RequestId}",
                    request.Email,
                    request.RequestId);

                return new AuthenticationResponseEvent
                {
                    RequestId = request.RequestId,
                    Success = false,
                    ErrorMessage = "Authentication service error",
                    RespondedAt = DateTime.UtcNow
                };
            }
        }
    }
}
