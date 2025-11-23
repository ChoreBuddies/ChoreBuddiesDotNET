namespace ChoreBuddies.Backend.Features.Notifications.Email;

public interface IEmailService
{
    public Task<string> SendRegisterConfirmationNotificationAsync(
        string recipientEmail,
        string recipientName,
        CancellationToken cancellationToken = default);
}
