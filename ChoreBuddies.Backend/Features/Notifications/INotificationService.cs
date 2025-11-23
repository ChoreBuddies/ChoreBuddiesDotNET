namespace ChoreBuddies.Backend.Features.Notifications;

public interface INotificationService
{
    public Task<string> SendNewChoreNotificationAsync(
        string recipientEmail,
        string recipientName,
        string choreName,
        string choreDescription,
        DateTime? dueDate,
        CancellationToken cancellationToken = default);
    public Task<string> SendNewRewardRequestNotificationAsync(
        string recipientEmail,
        string recipientName,
        string rewardName,
        string requester,
        CancellationToken cancellationToken = default);

}
