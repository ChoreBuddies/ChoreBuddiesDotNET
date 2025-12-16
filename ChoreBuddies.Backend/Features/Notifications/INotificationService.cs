using ChoreBuddies.Backend.Domain;

namespace ChoreBuddies.Backend.Features.Notifications;

public interface INotificationService
{
    Task<bool> SendNewChoreNotificationAsync(
        AppUser recipient,
        string choreName,
        string choreDescription,
        DateTime? dueDate,
        CancellationToken cancellationToken = default);

    Task<bool> SendNewRewardRequestNotificationAsync(
        AppUser recipient,
        string rewardName,
        string requester,
        CancellationToken cancellationToken = default);
    Task<bool> SendNewMessageNotificationAsync(
        AppUser recipient,
        string sender,
        string content,
        CancellationToken cancellationToken = default);
}

