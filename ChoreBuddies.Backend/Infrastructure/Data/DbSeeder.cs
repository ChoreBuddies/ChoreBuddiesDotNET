using ChoreBuddies.Backend.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Shared.Chores;
using Shared.Notifications;
using Shared.ScheduledChores;

namespace ChoreBuddies.Backend.Infrastructure.Data;

public class DbSeeder
{
    private readonly string[] roleNames = { "Adult", "Child" };
    private readonly string[] children = { "UserName3", "UserName4" };

    private readonly List<AppUser> _users;
    private readonly List<PredefinedChore> _defaultChores;
    private readonly List<PredefinedReward> _defaultRewards;

    public DbSeeder()
    {
        _users = generateUsers(15);
        _defaultChores = readDefaultChoresFromCsv("Infrastructure/Data/Csv/predefined_chores.csv");
        _defaultRewards = readDefaultRewardsFromCsv("Infrastructure/Data/Csv/predefined_rewards.csv");
    }

    public void SetUpDbSeeding(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSeeding((context, _) =>
            {
                SeedRoles(context);
                SeedUsers(context);
                SeedHouseholds(context);
                SeedDefaultChores(context);
                SeedChores(context);
                SeedScheduledChores(context);
                SeedRewards(context);
                SeedRedeemedRewards(context);
                SeedDefaultRewards(context);
            })
            .UseAsyncSeeding(async (context, _, ct) =>
            {
                await SeedRolesAsync(context, ct);
                await SeedUsersAsync(context, ct);
                await SeedHouseholdsAsync(context, ct);
                await SeedDefaultChoresAsync(context, ct);
                await SeedChoresAsync(context, ct);
                await SeedScheduledChoresAsync(context, ct);
                await SeedRewardsAsync(context, ct);
                await SeedRedeemedRewardsAsync(context, ct);
                await SeedDefaultRewardsAsync(context, ct);
            });
    }

    #region SeedMethods
    private void SeedRoles(DbContext context)
    {
        var roleManager = context.GetService<RoleManager<IdentityRole<int>>>();

        foreach (var roleName in roleNames)
        {
            if (!roleManager.RoleExistsAsync(roleName).Result)
            {
                roleManager.CreateAsync(new IdentityRole<int>(roleName)).Wait();
            }
        }
    }

    private async Task SeedRolesAsync(DbContext context, CancellationToken ct)
    {
        var roleManager = context.GetService<RoleManager<IdentityRole<int>>>();

        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<int>(roleName));
            }
        }
    }

    private void SeedUsers(DbContext context)
    {
        var userManager = context.GetService<UserManager<AppUser>>();
        foreach (var newUser in _users)
        {
            var result = userManager.CreateAsync(newUser, "Pass123!").Result;
            if (result.Succeeded)
            {
                AssignRoleToUser(userManager, newUser);
                var preferences = generateAndCustomizePreferences(newUser);
                context.Set<NotificationPreference>().AddRange(preferences);
            }
        }
        context.SaveChanges();
    }

    private async Task SeedUsersAsync(DbContext context, CancellationToken ct)
    {
        var userManager = context.GetService<UserManager<AppUser>>();
        foreach (var newUser in _users)
        {
            var result = await userManager.CreateAsync(newUser, "Pass123!");
            if (result.Succeeded)
            {
                await AssignRoleToUserAsync(userManager, newUser);
                var preferences = generateAndCustomizePreferences(newUser);
                await context.Set<NotificationPreference>().AddRangeAsync(preferences, ct);
            }
        }
        await context.SaveChangesAsync(ct);
    }

    private void AssignRoleToUser(UserManager<AppUser> userManager, AppUser user)
    {
        if (children.Contains(user.UserName))
        {
            userManager.AddToRoleAsync(user, "Child").Wait();
        }
        else
        {
            userManager.AddToRoleAsync(user, "Adult").Wait();
        }
    }

    private async Task AssignRoleToUserAsync(UserManager<AppUser> userManager, AppUser user)
    {
        if (children.Contains(user.UserName))
        {
            await userManager.AddToRoleAsync(user, "Child");
        }
        else
        {
            await userManager.AddToRoleAsync(user, "Adult");
        }
    }

    private void SeedHouseholds(DbContext context)
    {
        if (context.Set<Household>().Any()) return;

        var allUsers = context.Set<AppUser>().OrderBy(u => u.Id).ToList();

        if (allUsers.Count < 15) return;

        var households = createHouseholdScenarios(allUsers);

        context.Set<Household>().AddRange(households);
        context.SaveChanges();
    }

    private async Task SeedHouseholdsAsync(DbContext context, CancellationToken ct)
    {
        if (await context.Set<Household>().AnyAsync(ct)) return;

        var allUsers = await context.Set<AppUser>().OrderBy(u => u.Id).ToListAsync(ct);

        if (allUsers.Count < 15) return;

        var households = createHouseholdScenarios(allUsers);

        await context.Set<Household>().AddRangeAsync(households, ct);
        await context.SaveChangesAsync(ct);
    }

    private void SeedDefaultChores(DbContext context)
    {
        if (!context.Set<PredefinedChore>().Any())
        {
            context.Set<PredefinedChore>().AddRange(_defaultChores);
            context.SaveChanges();
        }
    }

    private async Task SeedDefaultChoresAsync(DbContext context, CancellationToken ct)
    {
        if (!await context.Set<PredefinedChore>().AnyAsync(ct))
        {
            await context.Set<PredefinedChore>().AddRangeAsync(_defaultChores, ct);
            await context.SaveChangesAsync(ct);
        }
    }

    private void SeedChores(DbContext context)
    {
        if (context.Set<Chore>().Any()) return;

        var households = context.Set<Household>().Include(h => h.Users).OrderBy(h => h.Id).ToList();

        if (households.Count < 3) return;

        var chores = createChoreScenarios(households);

        context.Set<Chore>().AddRange(chores);
        context.SaveChanges();
    }

    private async Task SeedChoresAsync(DbContext context, CancellationToken ct)
    {
        if (await context.Set<Chore>().AnyAsync(ct)) return;

        var households = await context.Set<Household>()
                                      .Include(h => h.Users)
                                      .OrderBy(h => h.Id)
                                      .ToListAsync(ct);

        if (households.Count < 3) return;

        var chores = createChoreScenarios(households);

        await context.Set<Chore>().AddRangeAsync(chores, ct);
        await context.SaveChangesAsync(ct);
    }

    private void SeedScheduledChores(DbContext context)
    {
        if (context.Set<ScheduledChore>().Any()) return;

        var households = context.Set<Household>().Include(h => h.Users).OrderBy(h => h.Id).ToList();
        if (households.Count < 3) return;

        var scheduledChores = createScheduledChoreScenarios(households);

        context.Set<ScheduledChore>().AddRange(scheduledChores);
        context.SaveChanges();
    }

    private async Task SeedScheduledChoresAsync(DbContext context, CancellationToken ct)
    {
        if (await context.Set<ScheduledChore>().AnyAsync(ct)) return;

        var households = await context.Set<Household>().Include(h => h.Users).OrderBy(h => h.Id).ToListAsync(ct);
        if (households.Count < 3) return;

        var scheduledChores = createScheduledChoreScenarios(households);

        await context.Set<ScheduledChore>().AddRangeAsync(scheduledChores, ct);
        await context.SaveChangesAsync(ct);
    }

    private void SeedRewards(DbContext context)
    {
        if (context.Set<Reward>().Any()) return;
        var households = context.Set<Household>().Include(h => h.Users).OrderBy(h => h.Id).ToList();
        if (households.Count < 3) return;

        var rewards = createRewardScenarios(households);
        context.Set<Reward>().AddRange(rewards);
        context.SaveChanges();
    }

    private async Task SeedRewardsAsync(DbContext context, CancellationToken ct)
    {
        if (await context.Set<Reward>().AnyAsync(ct)) return;
        var households = await context.Set<Household>().Include(h => h.Users).OrderBy(h => h.Id).ToListAsync(ct);
        if (households.Count < 3) return;

        var rewards = createRewardScenarios(households);
        await context.Set<Reward>().AddRangeAsync(rewards, ct);
        await context.SaveChangesAsync(ct);
    }

    private void SeedRedeemedRewards(DbContext context)
    {
        if (context.Set<RedeemedReward>().Any()) return;
        var households = context.Set<Household>().Include(h => h.Users).OrderBy(h => h.Id).ToList();
        if (households.Count < 3) return;

        var redeemed = createRedeemedRewardScenarios(households);
        context.Set<RedeemedReward>().AddRange(redeemed);
        context.SaveChanges();
    }

    private async Task SeedRedeemedRewardsAsync(DbContext context, CancellationToken ct)
    {
        if (await context.Set<RedeemedReward>().AnyAsync(ct)) return;
        var households = await context.Set<Household>().Include(h => h.Users).OrderBy(h => h.Id).ToListAsync(ct);
        if (households.Count < 3) return;

        var redeemed = createRedeemedRewardScenarios(households);
        await context.Set<RedeemedReward>().AddRangeAsync(redeemed, ct);
        await context.SaveChangesAsync(ct);
    }

    private void SeedDefaultRewards(DbContext context)
    {
        if (!context.Set<PredefinedReward>().Any())
        {
            context.Set<PredefinedReward>().AddRange(_defaultRewards);
            context.SaveChanges();
        }
    }

    private async Task SeedDefaultRewardsAsync(DbContext context, CancellationToken ct)
    {
        if (!await context.Set<PredefinedReward>().AnyAsync(ct))
        {
            await context.Set<PredefinedReward>().AddRangeAsync(_defaultRewards, ct);
            await context.SaveChangesAsync(ct);
        }
    }

    #endregion

    #region ScenarioMethods
    private List<Household> createHouseholdScenarios(List<AppUser> dbUsers)
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

    private List<NotificationPreference> generateAndCustomizePreferences(AppUser user)
    {
        var prefs = new List<NotificationPreference>();

        foreach (NotificationEvent eventType in Enum.GetValues(typeof(NotificationEvent)))
        {
            prefs.Add(new NotificationPreference
            {
                UserId = user.Id,
                Type = eventType,
                Channel = NotificationChannel.Email,
                IsEnabled = true,
                User = user
            });

            prefs.Add(new NotificationPreference
            {
                UserId = user.Id,
                Type = eventType,
                Channel = NotificationChannel.Push,
                IsEnabled = false,
                User = user
            });
        }

        if (user.Email == "user1@test.com")
        {
            foreach (var p in prefs)
            {
                p.IsEnabled = false;
            }
        }

        if (user.Email == "user2@test.com")
        {
            foreach (var p in prefs.Where(x => x.Channel == NotificationChannel.Push))
            {
                p.IsEnabled = true;
            }
        }

        if (user.Email == "user5@test.com")
        {
            foreach (var p in prefs) p.IsEnabled = false;

            var jobAssignedEmail = prefs.FirstOrDefault(p => p.Type == NotificationEvent.NewChore && p.Channel == NotificationChannel.Email);
            if (jobAssignedEmail != null) jobAssignedEmail.IsEnabled = true;
        }

        return prefs;
    }

    private List<PredefinedChore> readDefaultChoresFromCsv(string filePath)
    {
        var absolutePath = Path.Combine(AppContext.BaseDirectory, filePath);

        if (!File.Exists(absolutePath))
        {
            Console.Error.WriteLine($"Error: CSV file not found at path: {absolutePath}");
            return new List<PredefinedChore>();
        }

        try
        {
            return File.ReadAllLines(absolutePath)
                       .Skip(1)
                       .Where(line => !string.IsNullOrWhiteSpace(line))
                       .Select(line =>
                       {
                           string[] columns = line.Split(';');

                           // Parsing Frequency enum with error handling
                           if (!Enum.TryParse<Frequency>(columns[6].Trim(), true, out var frequency))
                           {
                               frequency = Frequency.Daily;
                           }

                           return new PredefinedChore()
                           {
                               Name = columns[1].Trim(),
                               Description = columns[2].Trim(),
                               Room = columns[3].Trim(),
                               RewardPointsCount = int.Parse(columns[4]),
                               ChoreDuration = int.Parse(columns[5]),
                               Frequency = frequency,
                               EveryX = int.Parse(columns[7])
                           };
                       }).ToList();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error parsing CSV file '{absolutePath}': {ex.Message}");
            return new List<PredefinedChore>();
        }
    }

    private List<PredefinedReward> readDefaultRewardsFromCsv(string filePath)
    {
        var absolutePath = Path.Combine(AppContext.BaseDirectory, filePath);

        if (!File.Exists(absolutePath))
        {
            Console.Error.WriteLine($"Error: CSV file not found at path: {absolutePath}");
            return new List<PredefinedReward>();
        }

        try
        {
            return File.ReadAllLines(absolutePath)
                       .Skip(1)
                       .Where(line => !string.IsNullOrWhiteSpace(line))
                       .Select(line =>
                       {
                           string[] columns = line.Split(';');
                           return new PredefinedReward
                           {
                               Name = columns[1].Trim(),
                               Description = columns[2].Trim(),
                               Cost = int.Parse(columns[3]),
                               QuantityAvailable = int.Parse(columns[4])
                           };
                       }).ToList();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error parsing CSV file '{absolutePath}': {ex.Message}");
            return new List<PredefinedReward>();
        }
    }

    private List<Chore> createChoreScenarios(List<Household> households)
    {
        var list = new List<Chore>();

        // households[0]
        // PUSTY

        // households[1]
        var hFamily = households[1];
        var usersFamily = hFamily.Users.ToList();

        if (usersFamily.Count >= 2)
        {
            list.Add(new Chore(
                "Naprawić kran",
                "Cieknie w kuchni",
                usersFamily[0].Id,
                hFamily.Id,
                DateTime.UtcNow.AddDays(2),
                Status.Assigned,
                "Kuchnia",
                50));

            list.Add(new Chore(
                "Wynieść śmieci",
                "Segregowane zmieszane",
                usersFamily[1].Id,
                hFamily.Id,
                DateTime.UtcNow.AddHours(4),
                Status.Assigned,
                "Garaż",
                10));

            list.Add(new Chore(
                "Zakupy spożywcze",
                "Lista na lodówce",
                usersFamily[0].Id,
                hFamily.Id,
                DateTime.UtcNow.AddDays(-1),
                Status.Completed,
                "Salon",
                20));
        }

        // households[2]
        var hStudent = households[2];
        var usersStudent = hStudent.Users.ToList();

        var random = new Random();
        var choreNames = new[] { "Zmywanie", "Odkurzanie", "Śmieci", "Zakupy", "Łazienka" };

        for (int i = 0; i < 50; i++)
        {
            var date = DateTime.UtcNow.AddDays(-i).AddHours(random.Next(-10, 10));
            var user = usersStudent[random.Next(usersStudent.Count)];
            var name = choreNames[random.Next(choreNames.Length)];

            list.Add(new Chore(
                name,
                "Wykonane w przeszłości",
                user.Id,
                hStudent.Id,
                date,
                Status.Completed,
                "Mieszkanie",
                random.Next(10, 50)
            ));
        }

        list.Add(new Chore("Kupić papier toaletowy", "Pilne!", null, hStudent.Id, DateTime.UtcNow, Status.Unassigned, "Łazienka", 15));
        list.Add(new Chore("Posprzątać po imprezie", "Butelki w salonie", null, hStudent.Id, DateTime.UtcNow.AddHours(10), Status.Unassigned, "Salon", 100));

        if (usersStudent.Any())
        {
            list.Add(new Chore("Zmyć naczynia", "Twoja kolej", usersStudent[0].Id, hStudent.Id, DateTime.UtcNow.AddHours(1), Status.Assigned, "Kuchnia", 25));
        }

        return list;
    }

    private List<ScheduledChore> createScheduledChoreScenarios(List<Household> households)
    {
        var list = new List<ScheduledChore>();

        // SCENARIUSZ 1: "Kawalerka" - PUSTE

        // SCENARIUSZ 2: "Rodzinka"
        var hFamily = households[1];

        list.Add(new ScheduledChore(
            "Mycie łazienki",
            "Szorowanie wanny i toalety",
            null,
            "Łazienka",
            1,
            Frequency.Weekly,
            100,
            hFamily.Id
        ));

        // SCENARIUSZ 3: "Akademik"
        var hStudent = households[2];

        list.Add(new ScheduledChore(
            "Śmieci",
            "Wyrzucić wór jak pełny",
            null,
            "Kuchnia",
            2,
            Frequency.Daily,
            10,
            hStudent.Id
        ));

        list.Add(new ScheduledChore(
            "Zbiórka na internet",
            "Przelew do gospodarza",
            hStudent.OwnerId,
            "Salon",
            1,
            Frequency.Monthly,
            0,
            hStudent.Id
        ));

        return list;
    }

    private List<Reward> createRewardScenarios(List<Household> households)
    {
        var list = new List<Reward>();

        // households[0]
        // PUSTY

        // households[1]
        var hFamily = households[1];
        list.Add(new Reward("Kieszonkowe", "20 PLN gotówką", hFamily.Id, 200, 10));
        list.Add(new Reward("Gra na konsoli", "1 godzina grania", hFamily.Id, 50, 999));
        list.Add(new Reward("Wyjście do kina", "Wspólne lub z kolegami", hFamily.Id, 500, 2));

        // households[2]
        var hStudent = households[2];
        list.Add(new Reward("Zwolnienie ze sprzątania", "Jednorazowy 'pass'", hStudent.Id, 300, 5));
        list.Add(new Reward("Pizza", "Składka na pizzę z funduszu domowego", hStudent.Id, 150, 10));
        list.Add(new Reward("Pierwszeństwo pod prysznicem", "Rano bez kolejki", hStudent.Id, 50, 20));
        list.Add(new Reward("Wybór filmu", "Ty wybierasz co oglądamy w piątek", hStudent.Id, 100, 4));

        return list;
    }

    private List<RedeemedReward> createRedeemedRewardScenarios(List<Household> households)
    {
        var list = new List<RedeemedReward>();

        // households[0]

        // households[1]
        var hFamily = households[1];
        var usersFamily = hFamily.Users.ToList();
        if (usersFamily.Count > 1)
        {
            var kidId = usersFamily[1].Id;
            list.Add(new RedeemedReward
            {
                Name = "Gra na konsoli",
                Description = "1 godzina grania",
                UserId = kidId,
                HouseholdId = hFamily.Id,
                RedeemedDate = DateTime.UtcNow.AddMinutes(-30),
                PointsSpent = 50,
                IsFulfilled = false
            });

            list.Add(new RedeemedReward
            {
                Name = "Kieszonkowe",
                Description = "20 PLN gotówką",
                UserId = kidId,
                HouseholdId = hFamily.Id,
                RedeemedDate = DateTime.UtcNow.AddDays(-7),
                PointsSpent = 200,
                IsFulfilled = true
            });
        }

        // households[2]
        var hStudent = households[2];
        var usersStudent = hStudent.Users.ToList();
        var random = new Random();

        var rewardTemplates = new[]
        {
            new { Name = "Zwolnienie ze sprzątania", Cost = 300 },
            new { Name = "Pizza", Cost = 150 },
            new { Name = "Pierwszeństwo pod prysznicem", Cost = 50 },
            new { Name = "Wybór filmu", Cost = 100 }
        };

        for (int i = 0; i < 30; i++)
        {
            var user = usersStudent[random.Next(usersStudent.Count)];
            var template = rewardTemplates[random.Next(rewardTemplates.Length)];

            list.Add(new RedeemedReward
            {
                Name = template.Name,
                Description = "Nagroda odebrana w przeszłości",
                UserId = user.Id,
                HouseholdId = hStudent.Id,
                RedeemedDate = DateTime.UtcNow.AddDays(-i).AddHours(random.Next(-5, 5)),
                PointsSpent = template.Cost,
                IsFulfilled = random.NextDouble() > 0.2
            });
        }

        return list;
    }

    #endregion
}
