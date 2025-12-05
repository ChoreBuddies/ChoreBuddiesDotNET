namespace ChoreBuddies.Backend.Features.Notifications.NotificationPreferences;

using ChoreBuddies.Backend.Domain;
using Shared.Notifications;

public interface INotificationPreferenceService
{
    Task<List<NotificationChannel>> GetActiveChannelsAsync(AppUser user, NotificationEvent eventType, CancellationToken ct = default);
    Task<List<NotificationPreference>> GetAllUserConfigAsync(AppUser user, CancellationToken ct = default);
    Task CreateDefaultPreferencesAsync(AppUser user, CancellationToken ct = default);
    Task UpdatePreferenceAsync(AppUser user, NotificationPreferenceDto updatedPreference, CancellationToken ct = default);
}
public class NotificationPreferenceService : INotificationPreferenceService
{
    private readonly INotificationPreferenceRepository _repository;

    public NotificationPreferenceService(INotificationPreferenceRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<NotificationChannel>> GetActiveChannelsAsync(AppUser user, NotificationEvent eventType, CancellationToken ct = default)
    {
        if (user == null) return new List<NotificationChannel>();

        if (user.NotificationPreferences != null && user.NotificationPreferences.Any())
        {
            return user.NotificationPreferences
                .Where(p => p.Type == eventType && p.IsEnabled)
                .Select(p => p.Channel)
                .Distinct()
                .ToList();
        }

        var dbPreferences = await _repository.GetEnabledChannelTypesAsync(user.Id, eventType, ct);

        bool hasExplicitSettings = await _repository.HasAnyPreferenceForEventAsync(user.Id, eventType, ct);

        if (!dbPreferences.Any() && !hasExplicitSettings)
        {
            return new List<NotificationChannel> { NotificationChannel.Email };
        }

        return dbPreferences;
    }
    public async Task CreateDefaultPreferencesAsync(AppUser user, CancellationToken ct = default)
    {
        var defaultPreferences = new List<NotificationPreference>();

        foreach (NotificationEvent eventType in Enum.GetValues(typeof(NotificationEvent)))
        {
            defaultPreferences.Add(new NotificationPreference
            {
                UserId = user.Id,
                Type = eventType,
                Channel = NotificationChannel.Email,
                IsEnabled = true,
                User = user,
            });
            defaultPreferences.Add(new NotificationPreference
            {
                UserId = user.Id,
                Type = eventType,
                Channel = NotificationChannel.Push,
                IsEnabled = false,
                User = user,
            });
        }

        await _repository.AddPreferencesAsync(defaultPreferences, ct);
    }

    public async Task<List<NotificationPreference>> GetAllUserConfigAsync(AppUser user, CancellationToken ct = default)
    {
        if (user == null) return new List<NotificationPreference>();

        IEnumerable<NotificationPreference> userPreferences;

        if (user.NotificationPreferences != null && user.NotificationPreferences.Any())
        {
            userPreferences = user.NotificationPreferences;
        }
        else
        {
            userPreferences = await _repository.GetAllUserPreferencesAsync(user.Id, ct);
        }
        return (List<NotificationPreference>)userPreferences;
    }
    public async Task UpdatePreferenceAsync(AppUser user, NotificationPreferenceDto updatedPreference, CancellationToken ct = default)
    {
        if (user == null || updatedPreference == null)
        {
            return;
        }

        var existingPreference = await _repository.GetPreferenceByKeysAsync(
            user.Id,
            updatedPreference.Type,
            updatedPreference.Channel,
            ct);

        if (existingPreference == null)
        {
            return;
        }

        existingPreference.IsEnabled = updatedPreference.IsEnabled;

        await _repository.UpdatePreferenceAsync(existingPreference, ct);
    }
}
