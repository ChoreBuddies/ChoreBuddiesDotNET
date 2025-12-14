using ChoreBuddies.Backend.Domain;
using FirebaseAdmin.Messaging;
using Shared.Notifications;

namespace ChoreBuddies.Backend.Features.Notifications.Push;

public class FirebaseNotificationsService : INotificationChannel
{
    public NotificationChannel ChannelType => throw new NotImplementedException();
    async Task<string> SendNotificationAsync(string userToken, string title, string body, Dictionary<string, string>? data = null)
    {
        var message = new Message()
        {
            Token = userToken,

            Notification = new Notification()
            {
                Title = title,
                Body = body
            },
        };

        try
        {
            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            return response;
        }
        catch (FirebaseMessagingException ex)
        {
            if (ex.MessagingErrorCode == MessagingErrorCode.Unregistered)
            {
            }
            throw;
        }
    }

    public async Task<string> SendNewChoreNotificationAsync(AppUser recipient, string choreName, string choreDescription, DateTime? dueDate, CancellationToken cancellationToken = default)
    {
        if (recipient.FcmToken is not null)
        {
            var title = "New Chore Available!";

            var dateString = dueDate.HasValue ? $"to {dueDate.Value:dd.MM}" : "no deadline";
            var body = $"{choreName} ({dateString}). {choreDescription}";

            var data = new Dictionary<string, string>
            {
            };

            return await SendNotificationAsync(recipient.FcmToken, title, body, data);
        }
        return "No token present";
    }

    public async Task<string> SendNewRewardRequestNotificationAsync(AppUser recipient, string rewardName, string requester, CancellationToken cancellationToken = default)
    {
        if (recipient.FcmToken is not null)
        {
            var title = "New Reward Request 🎁";
            var body = $"{requester} would like to recive: {rewardName}. Approve or deny.";

            var data = new Dictionary<string, string>
            {
            };

            return await SendNotificationAsync(recipient.FcmToken, title, body, data);
        }
        return "No token present";
    }
}
