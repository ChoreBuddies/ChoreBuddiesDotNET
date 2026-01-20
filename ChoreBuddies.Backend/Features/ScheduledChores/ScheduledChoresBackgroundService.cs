using ChoreBuddies.Backend.Domain;
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
        var now = DateTime.Now;
        var nextMidnight = now.Date.AddDays(1);
        var initialDelay = nextMidnight - now;
        await Task.Delay(initialDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ChoreBuddiesDbContext>();

            var periodicChores = await db.ScheduledChores.ToListAsync();

            foreach (var pc in periodicChores)
            {
                if (!ShouldGenerate(pc))
                    continue;

                DateTime dueDate = pc.Frequency switch
                {
                    Frequency.Daily => DateTime.Now.AddDays(1),
                    Frequency.Weekly => DateTime.Now.AddDays(7),
                    Frequency.Monthly => DateTime.Now.AddMonths(1),
                    _ => DateTime.Now
                };

                var chore = new Chore(
                    name: pc.Name,
                    description: pc.Description,
                    userId: null,
                    householdId: pc.HouseholdId,
                    dueDate: dueDate,
                    status: Shared.Chores.Status.Assigned,
                    room: pc.Room,
                    rewardPointsCount: pc.RewardPointsCount
                );

                db.Chores.Add(chore);

                pc.LastGenerated = DateTime.Now;
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

        return DateTime.Now >= next;
    }
}

