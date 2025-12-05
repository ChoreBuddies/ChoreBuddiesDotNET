namespace ChoreBuddies.Backend.Features.Notifications.UserNotificationPreferences;

using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public interface INotificationPreferenceRepository
{
    Task<List<NotificationChannel>> GetEnabledChannelTypesAsync(int userId, NotificationEvent eventType, CancellationToken ct = default);

    Task<bool> HasAnyPreferenceForEventAsync(int userId, NotificationEvent eventType, CancellationToken ct = default);
    Task AddPreferencesAsync(IEnumerable<NotificationPreference> preferences, CancellationToken ct = default);
}
public class NotificationPreferenceRepository : INotificationPreferenceRepository
{
    private readonly ChoreBuddiesDbContext _context;

    public NotificationPreferenceRepository(ChoreBuddiesDbContext context)
    {
        _context = context;
    }

    public async Task AddPreferencesAsync(IEnumerable<NotificationPreference> preferences, CancellationToken ct = default)
    {
        if (preferences == null || !preferences.Any())
        {
            return;
        }
        await _context.Set<NotificationPreference>().AddRangeAsync(preferences, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<NotificationChannel>> GetEnabledChannelTypesAsync(int userId, NotificationEvent eventType, CancellationToken ct = default)
    {
        return await _context.UserNotificationPreference
            .AsNoTracking()
            .Where(p => p.UserId == userId && p.Type == eventType && p.IsEnabled)
            .Select(p => p.Channel)
            .Distinct()
            .ToListAsync(ct);
    }

    public async Task<bool> HasAnyPreferenceForEventAsync(int userId, NotificationEvent eventType, CancellationToken ct = default)
    {
        return await _context.UserNotificationPreference
            .AsNoTracking()
            .AnyAsync(p => p.UserId == userId && p.Type == eventType, ct);
    }
}
