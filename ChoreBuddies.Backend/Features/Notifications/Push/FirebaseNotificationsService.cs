using ChoreBuddies.Backend.Domain;
using FirebaseAdmin.Messaging;
using Shared.Notifications;

namespace ChoreBuddies.Backend.Features.Notifications.Push;

public class FirebaseNotificationsService : INotificationChannel
{
    public NotificationChannel ChannelType => NotificationChannel.Push;
    async Task<string> SendNotificationAsync(AppUser user, string title, string body, Dictionary<string, string> data)
    {
        if (user.FcmToken is null) return "No token present";

        var message = new Message()
        {
            Token = user.FcmToken,

            Notification = new Notification()
            {
                Title = title,
                Body = body
            },
            Data = data,
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

    public async Task<string> SendNewChoreNotificationAsync(AppUser recipient, int choreId, string choreName, string choreDescription, DateTime? dueDate, CancellationToken cancellationToken = default)
    {

        var title = "New Chore Available!";

        var dateString = dueDate.HasValue ? $"to {dueDate.Value:dd.MM}" : "no deadline";
        var body = $"{choreName} ({dateString}). {choreDescription}";
        var data = new Dictionary<string, string>()
            {
                { "click_action", "FLUTTER_NOTIFICATION_CLICK" },
                { "screen", "/chore_details" },
                { "id", choreId.ToString() }
            };
        return await SendNotificationAsync(recipient, title, body, data);
    }

    public async Task<string> SendNewRewardRequestNotificationAsync(AppUser recipient, int rewardId, string rewardName, string requester, CancellationToken cancellationToken = default)
    {
        var title = "New Reward Request 🎁";
        var body = $"{requester} would like to recive: {rewardName}. Approve or deny.";
        var data = new Dictionary<string, string>()
            {
                { "click_action", "FLUTTER_NOTIFICATION_CLICK" },
                { "screen", "/reward_details" },
                { "id", rewardId.ToString() }
            };
        return await SendNotificationAsync(recipient, title, body, data);
    }

    public async Task<string> SendNewMessageNotificationAsync(AppUser recipient, string sender, string content, CancellationToken cancellationToken = default)
    {

        var title = $"New message from {sender}";
        var data = new Dictionary<string, string>()
            {
                { "click_action", "FLUTTER_NOTIFICATION_CLICK" },
                { "screen", "/chat" },
            };
        return await SendNotificationAsync(recipient, title, content, data);
    }

    public async Task<string> SendReminderNotificationAsync(AppUser recipient, int choreId, string choreName, CancellationToken cancellationToken = default)
    {

        var title = "Time is up!";
        var content = $"Let's {choreName}!";
        var data = new Dictionary<string, string>()
            {
                { "click_action", "FLUTTER_NOTIFICATION_CLICK" },
                { "screen", "/chore_details" },
                { "id", choreId.ToString() }
            };
        return await SendNotificationAsync(recipient, title, content, data);
    }

}
