using FIAP.CloudGames.Users.Application.Dtos;

namespace FIAP.CloudGames.Users.Application.Interfaces
{
    public interface IAuthService
    {
        Task<string> LoginAsync(LoginUserDto dto);
    }
}
