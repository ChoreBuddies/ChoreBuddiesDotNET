using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Households;
using ChoreBuddies.Backend.Features.Households.Exceptions;
using ChoreBuddies.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.PredefinedChores;

namespace ChoreBuddies.Backend.Features.ScheduledChores;

public class ScheduledChoresBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;

    public ScheduledChoresBackgroundService(IServiceProvider services)
    {
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var nextMidnight = DateTime.UtcNow.Date.AddDays(1);
        var initialDelay = nextMidnight - DateTime.UtcNow;
        await Task.Delay(initialDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ChoreBuddiesDbContext>();
            var householdService = scope.ServiceProvider.GetRequiredService<IHouseholdService>();

            var scheduledChores = await db.ScheduledChores.ToListAsync();

            foreach (var sc in scheduledChores)
            {
                if (!ShouldGenerate(sc))
                    continue;

                DateTime dueDate = sc.Frequency switch
                {
                    Frequency.Daily => DateTime.UtcNow.AddDays(1),
                    Frequency.Weekly => DateTime.UtcNow.AddDays(7),
                    Frequency.Monthly => DateTime.UtcNow.AddMonths(1),
                    _ => DateTime.UtcNow
                };

                var userId = sc.UserId;
                if (userId is null)
                {
                    try
                    {
                        userId = await householdService.GetUserIdForAutoAssignAsync(sc.Id);
                    }
                    catch (HouseholdDoesNotExistException)
                    {
                        var choresToDelete = db.ScheduledChores.Where(u => u.HouseholdId == sc.HouseholdId);
                        db.ScheduledChores.RemoveRange(choresToDelete);
                        continue;
                    }
                    catch (HouseholdHasNoUsersException)
                    {
                        userId = null;
                    }
                }
                var chore = new Chore(
                    name: sc.Name,
                    description: sc.Description,
                    userId: userId,
                    householdId: sc.HouseholdId,
                    dueDate: dueDate,
                    status: Shared.Chores.Status.Assigned,
                    room: sc.Room,
                    rewardPointsCount: sc.RewardPointsCount
                );

                db.Chores.Add(chore);

                sc.LastGenerated = DateTime.UtcNow;
            }

            await db.SaveChangesAsync();

            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }

    private bool ShouldGenerate(ScheduledChore pc)
    {
        if (!pc.LastGenerated.HasValue)
            return true;
        DateTime last = pc.LastGenerated.Value;

        DateTime next = pc.Frequency switch
        {
            Frequency.Daily => last.AddDays(pc.EveryX),
            Frequency.Weekly => last.AddDays(pc.EveryX * 7),
            Frequency.Monthly => last.AddMonths(pc.EveryX),
            _ => last
        };

        return DateTime.UtcNow >= next;
    }
}

