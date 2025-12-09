using FIAP.CloudGames.Users.Domain.Enums;
using FIAP.CloudGames.Users.Domain.ValueObjects;

namespace FIAP.CloudGames.Users.Domain.Entities;

public class User
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public Email Email { get; set; }

    public Password Password { get; set; }

    public UserRolesEnum Role { get; set; }

    public DateTime CreatedAt { get; set; }

    public User(string name, Email email, Password password, UserRolesEnum role)
    {
        Id = Guid.NewGuid();
        CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"));
        Name = name;
        Email = email;
        Password = password;
        Role = role;
    }

    private User() {  }
}
