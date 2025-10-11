using ChoreBuddies.Backend.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ChoreBuddies.Backend.Infrastructure.Data;

public class DbSeeder
{
    private readonly List<AppUser> _users;
    private readonly List<DefaultChore> _defaultChores;

    public DbSeeder()
    {
        _users = generateUsers(10);
        _defaultChores = ReadDefaultChoresFromCsv("Infrastructure/Data/Csv/default_chores.csv");
    }

    public void SetUpDbSeeding(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSeeding((context, _) =>
            {
                // Seed Users
                var userManager = context.GetService<UserManager<AppUser>>();
                foreach (var newUser in _users)
                {
                    var user = context.Set<AppUser>().FirstOrDefault(u => u.Email == newUser.Email);
                    if (user == null) userManager.CreateAsync(newUser, "Pass123!").Wait();
                }

                // Seed Default Chores if the table is empty
                if (!context.Set<DefaultChore>().Any())
                {
                    context.Set<DefaultChore>().AddRange(_defaultChores);
                    context.SaveChanges();
                }

            })
            .UseAsyncSeeding(async (context, _, ct) =>
            {
                // Seed Users Async
                var userManager = context.GetService<UserManager<AppUser>>();
                foreach (var newUser in _users)
                {
                    var user = await context.Set<AppUser>().FirstOrDefaultAsync(u => u.Email == newUser.Email, ct);
                    if (user == null) await userManager.CreateAsync(newUser, "Pass123!");
                }

                // Seed Default Chores Async if the table is empty
                if (!await context.Set<DefaultChore>().AnyAsync(ct))
                {
                    await context.Set<DefaultChore>().AddRangeAsync(_defaultChores, ct);
                    await context.SaveChangesAsync(ct);
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

    private List<DefaultChore> ReadDefaultChoresFromCsv(string filePath)
    {
        var absolutePath = Path.Combine(AppContext.BaseDirectory, filePath);

        if (!File.Exists(absolutePath))
        {
            Console.Error.WriteLine($"Error: CSV file not found at path: {absolutePath}");
            return new List<DefaultChore>();
        }

        try
        {
            return File.ReadAllLines(absolutePath)
                       .Skip(1)
                       .Where(line => !string.IsNullOrWhiteSpace(line))
                       .Select(line =>
                       {
                           string[] columns = line.Split(';');
                           return new DefaultChore
                           {
                               Name = columns[1].Trim(),
                               Description = columns[2].Trim(),
                               Frequency = columns[3].Trim(),
                               MinAge = int.Parse(columns[4]),
                               ChoreDuration = int.Parse(columns[5]),
                               RewardPointsCount = int.Parse(columns[6]),
                               Room = columns[7].Trim()
                           };
                       }).ToList();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error parsing CSV file '{absolutePath}': {ex.Message}");
            return new List<DefaultChore>();
        }
    }
}
