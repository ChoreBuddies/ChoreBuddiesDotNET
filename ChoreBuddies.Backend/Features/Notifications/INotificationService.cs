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

