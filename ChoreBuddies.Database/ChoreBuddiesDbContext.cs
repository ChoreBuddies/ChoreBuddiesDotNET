using Microsoft.EntityFrameworkCore;

namespace ChoreBuddies.Database;

public class ChoreBuddiesDbContext : DbContext
{
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
