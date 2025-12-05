using ChoreBuddies.Backend.Domain;
using Maileroo.DotNet.SDK;
using Shared.Notifications;

namespace ChoreBuddies.Backend.Features.Notifications.Email;

public class EmailServiceOptions
{
    public string From { get; set; } = default!;
    public string FromName { get; set; } = default!;
}

public class EmailService : INotificationChannel, IEmailService
{
    public NotificationChannel ChannelType => NotificationChannel.Email;

    NotificationChannel INotificationChannel.ChannelType => throw new NotImplementedException();

    private readonly MailerooClient _client;
    private readonly string _defaultFrom;
    private readonly string _defaultFromName;

    public EmailService(MailerooClient mailerooClient, string defaultFrom, string defaultFromName)
    {
        _client = mailerooClient;
        _defaultFrom = defaultFrom;
        _defaultFromName = defaultFromName;
    }

    public async Task<string> SendTemplatedEmailAsync(
        string toEmail,
        string toName,
        string templateId,
        string subject,
        Dictionary<string, object> parameters,
        CancellationToken cancellationToken = default)
    {
        var from = new EmailAddress(_defaultFrom, _defaultFromName);
        var to = new EmailAddress(toEmail, toName);

        var payload = new Dictionary<string, object?>
        {
            ["from"] = from,
            ["to"] = new List<EmailAddress> { to },
            ["template_id"] = templateId,
            ["subject"] = subject,
            ["template_data"] = parameters
        };

        var referenceId = await _client.SendTemplatedEmailAsync(payload, cancellationToken);
        return referenceId;
    }

    public async Task<string> SendNewChoreNotificationAsync(AppUser recipient, string choreName, string choreDescription, DateTime? dueDate, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(MailerooConstants.NewChoreTemplate))
            throw new ArgumentNullException(nameof(MailerooConstants.NewChoreTemplate), "Maileroo Template ID is required.");

        var parameters = new Dictionary<string, object>
        {
            { "choreName", choreName },
            { "choreDescription", choreDescription},
            { "dueDate", dueDate?.ToString("f") ?? "No due date" },
            { "recipientName", recipient.UserName}
        };

        return await SendTemplatedEmailAsync(
            recipient.Email,
            recipient.UserName,
            MailerooConstants.NewChoreTemplate,
            MailSubjects.NewChore,
            parameters,
            cancellationToken);
    }

    public async Task<string> SendRegisterConfirmationNotificationAsync(string recipientEmail, string recipientName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(MailerooConstants.RegisterConfirmationTemplate))
            throw new ArgumentNullException(nameof(MailerooConstants.RegisterConfirmationTemplate), "Maileroo Template ID is required.");

        var parameters = new Dictionary<string, object>
    {
        { "recipientName", recipientName }
    };
        return await SendTemplatedEmailAsync(
            recipientEmail,
            recipientName,
            MailerooConstants.RegisterConfirmationTemplate,
            MailSubjects.RegisterConfirmation,
            parameters,
            cancellationToken);
    }

    public async Task<string> SendNewRewardRequestNotificationAsync(AppUser recipient, string rewardName, string requester, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(MailerooConstants.NewRewardRequestTemplate))
            throw new ArgumentNullException(nameof(MailerooConstants.NewRewardRequestTemplate), "Maileroo Template ID is required.");

        var parameters = new Dictionary<string, object>
        {
            { "recipientName", recipient.UserName },
            { "rewardName", rewardName },
            { "requester", requester }
        };

        return await SendTemplatedEmailAsync(
            recipient.Email,
            recipient.UserName,
            MailerooConstants.NewRewardRequestTemplate,
            MailSubjects.NewRewardRequest,
            parameters,
            cancellationToken);
    }
}
