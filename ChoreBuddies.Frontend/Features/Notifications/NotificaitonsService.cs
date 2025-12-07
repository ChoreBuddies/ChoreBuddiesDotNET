using ChoreBuddies.Frontend.Utilities;
using Shared.Notifications;

namespace ChoreBuddies.Frontend.Features.Notifications;

public interface INotificationsService
{
    public Task<List<NotificationPreferenceDto>> GetNotificaitonPreferences();
    public Task<bool> UpdateNotifications(NotificationPreferenceDto preferences);
}

public class NotificationsService : INotificationsService
{
    private readonly HttpClientUtils _httpClientUtils;

    public NotificationsService(HttpClientUtils httpClientUtils)
    {
        _httpClientUtils = httpClientUtils;
    }

    public async Task<List<NotificationPreferenceDto>> GetNotificaitonPreferences()
    {
        return await _httpClientUtils.TryRequestAsync(
            () => _httpClientUtils.GetAsync<List<NotificationPreferenceDto>>(NotificationsConstants.ApiEndpointGetMyPreferences, true)
        ) ?? [];
    }
    public async Task<bool> UpdateNotifications(NotificationPreferenceDto preferences)
    {
        return await _httpClientUtils.TryRequestAsync(async () =>
        {
            await _httpClientUtils.PutAsync(NotificationsConstants.ApiEndpointUpdatePreferences, preferences, true);
            return true;
        });
    }
}

