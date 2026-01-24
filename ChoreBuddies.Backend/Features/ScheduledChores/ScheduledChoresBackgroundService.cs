using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Households;
using ChoreBuddies.Backend.Features.Households.Exceptions;
using ChoreBuddies.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Chores;
using Shared.PredefinedChores;
using Shared.Utilities;
namespace ChoreBuddies.Backend.Features.ScheduledChores;

public class ScheduledChoresBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly TimeProvider _timeProvider;

    public ScheduledChoresBackgroundService(IServiceProvider services, TimeProvider timeProvider)
    {
        _services = services;
        _timeProvider = timeProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var now = _timeProvider.GetUtcNow();
        var nextMidnight = now.Date.AddDays(1);
        var initialDelay = nextMidnight - now;
        await Task.Delay(initialDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var loopNow = _timeProvider.GetUtcNow();
            try
            {
                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ChoreBuddiesDbContext>();
                var householdService = scope.ServiceProvider.GetRequiredService<IHouseholdService>();

                var candidates = await db.ScheduledChores.AsNoTracking().Select(x => new { x.Id, x.Frequency, x.EveryX, x.LastGenerated }).ToListAsync(stoppingToken);
                var idsToProcess = new List<int>();

                foreach (var candidate in candidates)
                {
                    if (ShouldGenerate(candidate.LastGenerated, candidate.Frequency, candidate.EveryX, loopNow.Date))
                    {
                        idsToProcess.Add(candidate.Id);
                    }
                }
                if (idsToProcess.Any())
                {
                    var scheduledChores = await db.ScheduledChores
                        .Where(x => idsToProcess.Contains(x.Id))
                        .ToListAsync(stoppingToken);

                    foreach (var sc in scheduledChores)
                    {
                        DateTime dueDate = sc.Frequency switch
                        {
                            Frequency.Daily => loopNow.Date.AddDays(1),
                            Frequency.Weekly => loopNow.Date.AddWeeks(1),
                            Frequency.Monthly => loopNow.Date.AddMonths(1),
                            _ => loopNow.Date.AddDays(1)
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
                                db.ScheduledChores.Remove(sc);
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
                            status: userId is not null ? Status.Assigned : Status.Unassigned,
                            room: sc.Room,
                            rewardPointsCount: sc.RewardPointsCount
                        );

                        db.Chores.Add(chore);

                        sc.LastGenerated = loopNow.Date;
                    }

                    await db.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occured while processig Scheaduled Chores: {e}");
            }
            var endOfLoopTime = _timeProvider.GetUtcNow();
            var targetNextRun = loopNow.Date.AddDays(1);
            var delay = targetNextRun - endOfLoopTime;

            if (delay < TimeSpan.Zero)
            {
                delay = TimeSpan.Zero;
            }
            await _timeProvider.Delay(delay, stoppingToken);
        }
    }

    private bool ShouldGenerate(DateTime? lastGenerated, Frequency frequency, int everyX, DateTime now)
    {
        if (!lastGenerated.HasValue)
            return true;
        DateTime last = lastGenerated.Value;

        DateTime next = frequency switch
        {
            Frequency.Daily => last.AddDays(everyX),
            Frequency.Weekly => last.AddWeeks(everyX),
            Frequency.Monthly => last.AddMonths(everyX),
            _ => last
        };

        return now.Date >= next;
    }
}

