namespace ChoreBuddies.Backend.Features.Notifications.UserNotificationPreferences;

using ChoreBuddies.Backend.Domain;

public interface INotificationPreferenceService
{
    Task<List<NotificationChannel>> GetActiveChannelsAsync(AppUser user, NotificationEvent eventType, CancellationToken ct = default);
    Task CreateDefaultPreferencesAsync(AppUser user, CancellationToken ct = default); // ZMIANA
}
public class NotificationPreferenceService : INotificationPreferenceService // Używamy Twojego interfejsu
{
    private readonly INotificationPreferenceRepository _repository; // Wstrzykujemy Repozytorium

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
}
