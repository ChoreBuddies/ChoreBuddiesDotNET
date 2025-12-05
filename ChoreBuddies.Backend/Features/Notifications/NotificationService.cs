using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Notifications.UserNotificationPreferences;
using Microsoft.EntityFrameworkCore;

namespace ChoreBuddies.Backend.Features.Notifications;

public class NotificationService : INotificationService
{
    private readonly IEnumerable<INotificationChannel> _channels;
    private readonly INotificationPreferenceService _preferenceService;

    public NotificationService(
        IEnumerable<INotificationChannel> channels,
        INotificationPreferenceService preferenceService)
    {
        _channels = channels;
        _preferenceService = preferenceService;
    }

    private IEnumerable<INotificationChannel> GetChannelsToExecute(IEnumerable<NotificationChannel> requiredChannels)
    {
        return _channels.Where(c => requiredChannels.Contains(c.ChannelType));
    }

    public async Task<bool> SendNewChoreNotificationAsync(AppUser recipient, string choreName, string choreDescription, DateTime? dueDate, CancellationToken ct = default)
    {
        var requiredChannels = await _preferenceService.GetActiveChannelsAsync(recipient, NotificationEvent.NewChore, ct);

        var channelsToUse = GetChannelsToExecute(requiredChannels);

        int successCount = 0;
        foreach (var channel in channelsToUse)
        {
            try
            {
                await channel.SendNewChoreNotificationAsync(recipient, choreName, choreDescription, dueDate, ct);
                successCount++;
            }
            catch (Exception)
            {
                throw;
            }
        }

        return true;
    }

    public async Task<bool> SendNewRewardRequestNotificationAsync(AppUser recipient, string rewardName, string requester, CancellationToken ct = default)
    {
        var requiredChannels = await _preferenceService.GetActiveChannelsAsync(recipient, NotificationEvent.RewardRequest, ct);
        var channelsToUse = GetChannelsToExecute(requiredChannels);

        int successCount = 0;
        foreach (var channel in channelsToUse)
        {
            try
            {
                await channel.SendNewRewardRequestNotificationAsync(recipient, rewardName, requester, ct);
                successCount++;
            }
            catch (Exception)
            {
                throw;
            }
        }

        return true;
    }
}
