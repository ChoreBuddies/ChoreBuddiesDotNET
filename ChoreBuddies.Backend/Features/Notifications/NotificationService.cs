using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Notifications.NotificationPreferences;
using ChoreBuddies.Backend.Features.Users;
using Microsoft.EntityFrameworkCore;
using Shared.Notifications;

namespace ChoreBuddies.Backend.Features.Notifications;

public class NotificationService : INotificationService
{
    private readonly IEnumerable<INotificationChannel> _channels;
    private readonly INotificationPreferenceService _preferenceService;
    private readonly IAppUserService _appUserService;

    public NotificationService(
        IEnumerable<INotificationChannel> channels,
        INotificationPreferenceService preferenceService,
        IAppUserService appUserService)
    {
        _channels = channels;
        _preferenceService = preferenceService;
        _appUserService = appUserService;
    }

    private IEnumerable<INotificationChannel> GetChannelsToExecute(IEnumerable<NotificationChannel> requiredChannels)
    {
        return _channels.Where(c => requiredChannels.Contains(c.ChannelType));
    }

    private async Task<AppUser> getRecipient(int recipientId)
    {
        var recipient = await _appUserService.GetUserByIdAsync(recipientId);
        if (recipient == null)
        {
            throw new ArgumentException("Invalid recipient");
        }
        return recipient!;
    }

    public async Task<bool> SendNewChoreNotificationAsync(int recipientId, string choreName, string choreDescription, DateTime? dueDate, CancellationToken ct = default)
    {
        var recipient = await getRecipient(recipientId);
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

    public async Task<bool> SendNewRewardRequestNotificationAsync(int recipientId, string rewardName, string requester, CancellationToken ct = default)
    {
        var recipient = await getRecipient(recipientId);

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

    public async Task<bool> SendNewMessageNotificationAsync(int recipientId, string sender, string content, CancellationToken cancellationToken = default)
    {
        var recipient = await getRecipient(recipientId);

        var requiredChannels = await _preferenceService.GetActiveChannelsAsync(recipient, NotificationEvent.NewMessage, cancellationToken);
        var channelsToUse = GetChannelsToExecute(requiredChannels);

        int successCount = 0;
        foreach (var channel in channelsToUse)
        {
            try
            {
                await channel.SendNewMessageNotificationAsync(recipient, sender, content, cancellationToken);
                successCount++;
            }
            catch (Exception)
            {
                throw;
            }
        }

        return true;
    }

    public async Task<bool> SendReminderAsync(int recipientId, string choreName, CancellationToken cancellationToken = default)
    {
        var recipient = await getRecipient(recipientId);

        var requiredChannels = await _preferenceService.GetActiveChannelsAsync(recipient, NotificationEvent.NewMessage, cancellationToken);
        var channelsToUse = GetChannelsToExecute(requiredChannels);

        foreach (var channel in channelsToUse)
        {
            try
            {
                await channel.SendReminderNotificationAsync(recipient, choreName, cancellationToken);
            }
            catch (Exception)
            {
                throw;
            }
        }

        return true;
    }
}
