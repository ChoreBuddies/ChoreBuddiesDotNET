using ChoreBuddies.Backend.Domain;

namespace ChoreBuddies.Backend.Features.Notifications.Email;

public interface IEmailService
{
    public Task<string> SendRegisterConfirmationNotificationAsync(
        AppUser recipientEmail,
        string recipientName,
        CancellationToken cancellationToken = default);
}
