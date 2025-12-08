using ChoreBuddies.Backend.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChoreBuddies.Backend.Infrastructure.Data;

public class ChoreBuddiesDbContext : IdentityDbContext<AppUser, IdentityRole<int>, int>
{
    public DbSet<AppUser> ApplicationUsers { get; set; }
    public DbSet<Chore> Chores { get; set; }
    public DbSet<DefaultChore> DefaultChores { get; set; }
    public DbSet<Household> Households { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<NotificationPreference> NotificationPreference { get; set; }
    public DbSet<ScheduledChore> ScheduledChores { get; set; }

    public ChoreBuddiesDbContext(DbContextOptions<ChoreBuddiesDbContext> options)
        : base(options)
    {
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        new DbSeeder().SetUpDbSeeding(optionsBuilder);
    }
}
