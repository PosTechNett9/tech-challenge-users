using FIAP.CloudGames.Users.Domain.Entities;
using FIAP.CloudGames.Users.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FIAP.CloudGames.Users.Infrastructure.Context;

public class UsersDbContext(DbContextOptions<UsersDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .Property(u => u.Password)
            .HasConversion(
                v => v.Hash,
                v => Password.FromHashed(v)
            );

        modelBuilder.Entity<User>()
            .Property(u => u.Email)
            .HasConversion(
                v => v.Address,
                v => Email.Create(v)
            );
    }
}
