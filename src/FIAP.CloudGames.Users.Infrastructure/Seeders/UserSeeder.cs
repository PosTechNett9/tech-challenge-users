using FIAP.CloudGames.Users.Domain.Entities;
using FIAP.CloudGames.Users.Domain.Enums;
using FIAP.CloudGames.Users.Domain.ValueObjects;
using FIAP.CloudGames.Users.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace FIAP.CloudGames.Users.Infrastructure.Seeders;

public static class UserSeeder
{
    public static async Task SeedAdminAsync(UsersDbContext context)
    {
        if (await context.Users.AnyAsync(u => u.Role == UserRolesEnum.Admin))
            return;

        var adminUser = new User(
            name: "Admin",
            email: Email.Create("admin@cloudgames.com"),
            password: Password.FromPlainText("Admin@1234!"),
            role: UserRolesEnum.Admin
        );

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();
    }
}
