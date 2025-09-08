using Microsoft.EntityFrameworkCore;

namespace ChoreBuddies.Database;

public class DbSeeder
{
    public DbSeeder()
    {
    }

    public void SetUpDbSeeding(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSeeding((context, _) =>
            {

            })
            .UseAsyncSeeding(async (context, _, ct) =>
            {
            });
    }
}
