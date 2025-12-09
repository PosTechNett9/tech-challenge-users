using FIAP.CloudGames.Users.Domain.Enums;

namespace FIAP.CloudGames.Users.Application.Dtos
{
    public class CreateUserDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public UserRolesEnum Role { get; set; }
    }
}
