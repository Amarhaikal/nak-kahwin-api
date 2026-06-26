using Microsoft.EntityFrameworkCore;
using nak_kahwin_api.Models;

namespace nak_kahwin_api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<ChecklistGroup> ChecklistGroups => Set<ChecklistGroup>();
    public DbSet<ChecklistItem> ChecklistItems => Set<ChecklistItem>();
    public DbSet<SavingsGoal> SavingsGoals => Set<SavingsGoal>();
    public DbSet<SavingsContribution> SavingsContributions => Set<SavingsContribution>();

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

        builder.Entity<ChecklistGroup>(cg =>
        {
            cg.HasKey(x => x.Id);
            cg.HasOne(cg => cg.Event)
              .WithMany()
              .HasForeignKey(x => x.EventId)
              .OnDelete(DeleteBehavior.Cascade);
            cg.HasIndex(x => new { x.EventId, x.Order });
        });

        builder.Entity<ChecklistItem>(ci =>
        {
            ci.HasKey(x => x.Id);
            ci.HasOne(ci => ci.Group)
              .WithMany(g => g.Items)
              .HasForeignKey(x => x.GroupId)
              .OnDelete(DeleteBehavior.Cascade);
            ci.HasIndex(x => new { x.GroupId, x.Order });
        });

        builder.Entity<SavingsGoal>(sg =>
        {
            sg.HasKey(x => x.Id);
            sg.HasOne(sg => sg.Event)
              .WithMany()
              .HasForeignKey(x => x.EventId)
              .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SavingsContribution>(sc =>
        {
            sc.HasKey(x => x.Id);
            sc.HasOne(sc => sc.SavingsGoal)
              .WithMany(g => g.Contributions)
              .HasForeignKey(x => x.GoalId)
              .OnDelete(DeleteBehavior.Cascade);
            sc.HasOne(sc => sc.Contributor)
              .WithMany()
              .HasForeignKey(x => x.ContributorId)
              .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
