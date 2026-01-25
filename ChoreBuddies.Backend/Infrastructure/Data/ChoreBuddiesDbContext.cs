using ChoreBuddies.Backend.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChoreBuddies.Backend.Infrastructure.Data;

public class ChoreBuddiesDbContext : IdentityDbContext<AppUser, IdentityRole<int>, int>
{
    public DbSet<AppUser> ApplicationUsers { get; set; }
    public DbSet<Chore> Chores { get; set; }
    public DbSet<PredefinedChore> PredefinedChores { get; set; }
    public DbSet<Household> Households { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<NotificationPreference> NotificationPreference { get; set; }
    public DbSet<PredefinedReward> PredefinedRewards { get; set; }
    public DbSet<RedeemedReward> RedeemedRewards { get; set; }
    public DbSet<Reward> Rewards { get; set; }
    public DbSet<ScheduledChore> ScheduledChores { get; set; }
    private readonly TimeProvider _timeProvider;

    public ChoreBuddiesDbContext(DbContextOptions<ChoreBuddiesDbContext> options, TimeProvider timeProvider)
        : base(options)
    {
        _timeProvider = timeProvider;
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var isDevelopment = string.Equals(envName, "Development", StringComparison.OrdinalIgnoreCase);

        new DbSeeder().SetUpDbSeeding(optionsBuilder, isDevelopment);
    }
    public override int SaveChanges()
    {
        SetAuditProperties();
        return base.SaveChanges();
    }
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditProperties();
        return base.SaveChangesAsync(cancellationToken);
    }
    private void SetAuditProperties()
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var entries = ChangeTracker.Entries<Chore>().Where(e => e.State == EntityState.Modified || e.State == EntityState.Added);
        foreach (var entry in entries)
        {
            entry.Entity.LastEditDate = now;
        }
    }
}
