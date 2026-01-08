using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Notifications.NotificationPreferences;
using ChoreBuddies.Backend.Features.Users;
using Shared.Notifications;

namespace ChoreBuddies.Backend.Features.Notifications;

public interface INotificationService
{
    Task<bool> SendNewChoreNotificationAsync(
        int recipientId,
        string choreName,
        string choreDescription,
        DateTime? dueDate,
        CancellationToken cancellationToken = default);

    Task<bool> SendNewRewardRequestNotificationAsync(
        int recipientId,
        string rewardName,
        string requester,
        CancellationToken cancellationToken = default);
    Task<bool> SendNewMessageNotificationAsync(
        int recipientId,
        string sender,
        string content,
        CancellationToken cancellationToken = default);
    Task<bool> SendReminderAsync(
        int recipientId,
        string content,
        CancellationToken cancellationToken = default);
}

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

    private async Task<AppUser> getRecipient(int recipientId)
    {
        var recipient = await _appUserService.GetUserByIdAsync(recipientId);
        if (recipient == null)
        {
            throw new ArgumentException("Invalid recipient");
        }
        return recipient!;
    }

    private async Task<bool> SendNotificationAsync(int recipientId, NotificationEvent eventType, Func<INotificationChannel, AppUser, Task> sender, CancellationToken ct = default)
    {

        var recipient = await getRecipient(recipientId);
        var requiredChannels = await _preferenceService.GetActiveChannelsAsync(recipient, eventType, ct);

        var channelsToUse = _channels.Where(c => requiredChannels.Contains(c.ChannelType));

        int successCount = 0;
        foreach (var channel in channelsToUse)
        {
            await sender(channel, recipient);
            successCount++;
        }

        return true;
    }

    public async Task<bool> SendNewChoreNotificationAsync(int recipientId, string choreName, string choreDescription, DateTime? dueDate, CancellationToken ct = default)
    {
        return await SendNotificationAsync(recipientId, NotificationEvent.NewChore, (channel, recipient) => channel.SendNewChoreNotificationAsync(recipient, choreName, choreDescription, dueDate, ct));
    }

    public async Task<bool> SendNewRewardRequestNotificationAsync(int recipientId, string rewardName, string requester, CancellationToken ct = default)
    {
        return await SendNotificationAsync(recipientId, NotificationEvent.RewardRequest, (channel, recipient) => channel.SendNewRewardRequestNotificationAsync(recipient, rewardName, requester, ct));
    }

    public async Task<bool> SendNewMessageNotificationAsync(int recipientId, string sender, string content, CancellationToken cancellationToken = default)
    {
        return await SendNotificationAsync(recipientId, NotificationEvent.NewMessage, (channel, recipient) => channel.SendNewMessageNotificationAsync(recipient, sender, content, cancellationToken));

    }

    public async Task<bool> SendReminderAsync(int recipientId, string choreName, CancellationToken cancellationToken = default)
    {
        return await SendNotificationAsync(recipientId, NotificationEvent.Reminder, (channel, recipient) => channel.SendReminderNotificationAsync(recipient, choreName, cancellationToken));

    }
}
