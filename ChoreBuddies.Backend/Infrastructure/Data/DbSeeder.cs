using ChoreBuddies.Backend.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ChoreBuddies.Backend.Infrastructure.Data;

public class DbSeeder
{
    private readonly List<AppUser> _users;
    public DbSeeder()
    {
        _users = generateUsers(10);
    }

    public void SetUpDbSeeding(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSeeding((context, _) =>
            {
                var userManager = context.GetService<UserManager<AppUser>>();

                foreach (var newUser in _users)
                {
                    var user = context.Set<AppUser>().FirstOrDefault(u => u.Email == newUser.Email);
                    if (user == null) userManager.CreateAsync(newUser, "Pass123!").Wait();
                }

            })
            .UseAsyncSeeding(async (context, _, ct) =>
            {
                var userManager = context.GetService<UserManager<AppUser>>();

                foreach (var newUser in _users)
                {
                    var user = await context.Set<AppUser>().FirstOrDefaultAsync(u => u.Email == newUser.Email);
                    if (user == null) await userManager.CreateAsync(newUser, "Pass123!");
                }
            });
    }

    private List<AppUser> generateUsers(int n)
    {
        var users = new List<AppUser>();
        for (var i = 0; i < n; i++)
            users.Add(new AppUser { Email = $"user{i + 1}@test.com", UserName = $"UserName{i + 1}" });

        return users;
    }
}
