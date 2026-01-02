using ChoreBuddies.Backend.Domain;
using Shared.Notifications;

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
    Task<string> SendNewMessageNotificationAsync(
    AppUser recipient,
    string sender,
    string content,
    CancellationToken cancellationToken = default);
    Task<string> SendReminderNotificationAsync(
AppUser recipient,
string choreName,
CancellationToken cancellationToken = default);
}
