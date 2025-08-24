using Microsoft.EntityFrameworkCore;

namespace ChoreBuddies.Database;

public class ChoreBuddiesDbContext : DbContext
{
    public ChoreBuddiesDbContext() { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        new DbSeeder().SetUpDbSeeding(optionsBuilder);
    }
}
