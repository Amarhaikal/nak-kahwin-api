using Microsoft.EntityFrameworkCore;
using nak_kahwin_api.Models;

namespace nak_kahwin_api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Event> Events => Set<Event>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>(u =>
        {
            u.HasKey(x => x.Id);
            u.HasIndex(x => x.Email).IsUnique();
        });

        builder.Entity<RefreshToken>(rt =>
        {
            rt.HasKey(x => x.Id);
            rt.HasIndex(x => x.Token).IsUnique();
            rt.HasOne(x => x.User)
              .WithMany()
              .HasForeignKey(x => x.UserId)
              .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Event>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Owner)
              .WithMany()
              .HasForeignKey(x => x.OwnerId)
              .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Partner)
              .WithMany()
              .HasForeignKey(x => x.PartnerId)
              .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
