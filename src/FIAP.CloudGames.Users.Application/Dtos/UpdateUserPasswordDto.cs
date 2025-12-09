namespace FIAP.CloudGames.Users.Application.Dtos
{
    public class UpdateUserPasswordDto
    {
        public required Guid Id { get; set; }
        public string Password { get; set; }
    }
}
