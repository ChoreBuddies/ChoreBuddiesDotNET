using ChoreBuddies.Backend.Domain;

namespace ChoreBuddies.Backend.Features.Notifications;

public interface INotificationChannel
{
    NotificationChannel ChannelType { get; }

    Task<string> SendNewChoreNotificationAsync(
        AppUser recipient,
        string choreName,
        string choreDescription,
        DateTime? dueDate,
        CancellationToken cancellationToken = default);

    Task<string> SendNewRewardRequestNotificationAsync(
        AppUser recipient,
        string rewardName,
        string requester,
        CancellationToken cancellationToken = default);
}
