namespace ChoreBuddies.Backend.Features.Notifications.NotificationPreferences;

using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Notifications;

public interface INotificationPreferenceRepository
{
    Task<List<NotificationChannel>> GetEnabledChannelTypesAsync(int userId, NotificationEvent eventType, CancellationToken ct = default);

    Task<bool> HasAnyPreferenceForEventAsync(int userId, NotificationEvent eventType, CancellationToken ct = default);
    Task AddPreferencesAsync(IEnumerable<NotificationPreference> preferences, CancellationToken ct = default);
    Task<List<NotificationPreference>> GetAllUserPreferencesAsync(int userId, CancellationToken ct = default);
    Task UpdatePreferenceAsync(NotificationPreference preferenceToUpdate, CancellationToken ct = default);
    Task<NotificationPreference?> GetPreferenceByKeysAsync(int userId, NotificationEvent type, NotificationChannel channel, CancellationToken ct = default);
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
        return await _context.NotificationPreference
            .AsNoTracking()
            .Where(p => p.UserId == userId && p.Type == eventType && p.IsEnabled)
            .Select(p => p.Channel)
            .Distinct()
            .ToListAsync(ct);
    }

    public async Task<bool> HasAnyPreferenceForEventAsync(int userId, NotificationEvent eventType, CancellationToken ct = default)
    {
        return await _context.NotificationPreference
            .AsNoTracking()
            .AnyAsync(p => p.UserId == userId && p.Type == eventType, ct);
    }
    public async Task<List<NotificationPreference>> GetAllUserPreferencesAsync(int userId, CancellationToken ct = default)
    {
        return await _context.NotificationPreference
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .ToListAsync(ct);
    }
    public async Task<NotificationPreference?> GetPreferenceByKeysAsync(int userId, NotificationEvent type, NotificationChannel channel, CancellationToken ct = default)
    {
        return await _context.NotificationPreference
            .FirstOrDefaultAsync(p =>
                p.UserId == userId &&
                p.Type == type &&
                p.Channel == channel, ct);
    }

    public async Task UpdatePreferenceAsync(NotificationPreference preferenceToUpdate, CancellationToken ct = default)
    {
        _context.Set<NotificationPreference>().Update(preferenceToUpdate);

        await _context.SaveChangesAsync(ct);
    }
}
