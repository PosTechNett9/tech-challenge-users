namespace FIAP.CloudGames.Users.Application.Dtos
{
    public class AuthenticationRequestEvent
    {
        public Guid RequestId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
    }

    public class AuthenticationResponseEvent
    {
        public Guid RequestId { get; set; }
        public bool Success { get; set; }
        public Guid? UserId { get; set; }
        public string? Token { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime RespondedAt { get; set; }
    }
}
