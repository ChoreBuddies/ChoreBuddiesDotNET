using FirebaseAdmin.Messaging;

namespace ChoreBuddies.Backend.Features.Notifications.Push;

public interface IFirebaseMessagingClient
{
    Task<string> SendAsync(Message message, CancellationToken cancellationToken = default);
}

public class FirebaseClientAdapter : IFirebaseMessagingClient
{
    public Task<string> SendAsync(Message message, CancellationToken cancellationToken = default)
    {
        return FirebaseMessaging.DefaultInstance.SendAsync(message, cancellationToken);
    }
}

