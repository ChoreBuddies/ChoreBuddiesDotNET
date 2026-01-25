using Maileroo.DotNet.SDK;

namespace ChoreBuddies.Backend.Features.Notifications.Email;

public interface IMailerooClient
{
    Task<string> SendTemplatedEmailAsync(
        Dictionary<string, object?> payload,
        CancellationToken cancellationToken = default);
}
public class MailerooClientAdapter : IMailerooClient
{
    private readonly MailerooClient _client;

    public MailerooClientAdapter(MailerooClient client)
    {
        _client = client;
    }

    public Task<string> SendTemplatedEmailAsync(
        Dictionary<string, object?> payload,
        CancellationToken cancellationToken = default)
    {
        return _client.SendTemplatedEmailAsync(payload, cancellationToken);
    }
}
