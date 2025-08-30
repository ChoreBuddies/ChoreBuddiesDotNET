using ChoreBuddies.Backend.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChoreBuddies.Database;

public class ChoreBuddiesDbContext : IdentityDbContext<ApplicationUser>
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
