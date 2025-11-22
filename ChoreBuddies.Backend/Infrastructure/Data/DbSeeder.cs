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
        _users = generateUsers(15);
        _defaultChores = readDefaultChoresFromCsv("Infrastructure/Data/Csv/default_chores.csv");
    }

    public void SetUpDbSeeding(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSeeding((context, _) =>
            {
                SeedUsers(context);
                SeedHouseholds(context);
                SeedDefaultChores(context);
            })
            .UseAsyncSeeding(async (context, _, ct) =>
            {
                await SeedUsersAsync(context, ct);
                await SeedHouseholdsAsync(context, ct);
                await SeedDefaultChoresAsync(context, ct);
            });
    }

    #region private
    private void SeedUsers(DbContext context)
    {
        var userManager = context.GetService<UserManager<AppUser>>();
        foreach (var newUser in _users)
        {
            var user = context.Set<AppUser>().FirstOrDefault(u => u.Email == newUser.Email);
            if (user == null) userManager.CreateAsync(newUser, "Pass123!").Wait();
        }
    }

    private async Task SeedUsersAsync(DbContext context, CancellationToken ct)
    {
        var userManager = context.GetService<UserManager<AppUser>>();
        foreach (var newUser in _users)
        {
            var user = await context.Set<AppUser>().FirstOrDefaultAsync(u => u.Email == newUser.Email, ct);
            if (user == null) await userManager.CreateAsync(newUser, "Pass123!");
        }
    }

    private void SeedHouseholds(DbContext context)
    {
        if (context.Set<Household>().Any()) return;

        var allUsers = context.Set<AppUser>().OrderBy(u => u.Id).ToList();

        if (allUsers.Count < 15) return;

        var households = CreateHouseholdScenarios(allUsers);

        context.Set<Household>().AddRange(households);
        context.SaveChanges();
    }

    private async Task SeedHouseholdsAsync(DbContext context, CancellationToken ct)
    {
        if (await context.Set<Household>().AnyAsync(ct)) return;

        var allUsers = await context.Set<AppUser>().OrderBy(u => u.Id).ToListAsync(ct);

        if (allUsers.Count < 15) return;

        var households = CreateHouseholdScenarios(allUsers);

        await context.Set<Household>().AddRangeAsync(households, ct);
        await context.SaveChangesAsync(ct);
    }

    private void SeedDefaultChores(DbContext context)
    {
        if (!context.Set<DefaultChore>().Any())
        {
            context.Set<DefaultChore>().AddRange(_defaultChores);
            context.SaveChanges();
        }
    }

    private async Task SeedDefaultChoresAsync(DbContext context, CancellationToken ct)
    {
        if (!await context.Set<DefaultChore>().AnyAsync(ct))
        {
            await context.Set<DefaultChore>().AddRangeAsync(_defaultChores, ct);
            await context.SaveChangesAsync(ct);
        }
    }

    private List<Household> CreateHouseholdScenarios(List<AppUser> dbUsers)
    {
        var households = new List<Household>();

        // Scenariusz 1: "Kawalerka" (1 użytkownik)
        // Używamy dbUsers[0] jako właściciela
        var owner1 = dbUsers[0];
        var h1 = new Household(owner1.Id, "Mieszkanie Marka", "ABC001", "Moja prywatna jaskinia");
        h1.Users.Add(owner1); // Dodajemy użytkownika do kolekcji, EF zaktualizuje FK
        households.Add(h1);

        // Scenariusz 2: "Rodzinka" (Mała grupa: Właściciel + 2 osoby)
        // Używamy dbUsers[1] jako właściciela, dbUsers[2] i [3] jako członków
        var owner2 = dbUsers[1];
        var h2 = new Household(owner2.Id, "Rodzina Nowaków", "FAM002", "Rodzice i syn");
        h2.Users.Add(owner2);
        h2.Users.Add(dbUsers[2]);
        h2.Users.Add(dbUsers[3]);
        households.Add(h2);

        // Scenariusz 3: "Akademik" (Duża grupa: Właściciel + 6 osób)
        // Używamy dbUsers[4] jako właściciela, dbUsers[5]...[10] jako członków
        var owner3 = dbUsers[4];
        var h3 = new Household(owner3.Id, "Akademik Pokój 101", "STU003", "Imprezownia i sprzątanie w piątki");
        h3.Users.Add(owner3);
        for (int i = 5; i <= 10; i++)
        {
            h3.Users.Add(dbUsers[i]);
        }
        households.Add(h3);

        return households;
    }

    private List<AppUser> generateUsers(int n)
    {
        var users = new List<AppUser>();
        for (var i = 0; i < n; i++)
            users.Add(new AppUser
            {
                Email = $"user{i + 1}@test.com",
                UserName = $"UserName{i + 1}",
                FirstName = $"User{i + 1}",
                LastName = "Testowy"
            });

        return users;
    }

    private List<DefaultChore> readDefaultChoresFromCsv(string filePath)
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
    #endregion
}
