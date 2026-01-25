using ChoreBuddies.Backend.Domain;
using Maileroo.DotNet.SDK;
using Microsoft.Extensions.Options;
using Shared.Notifications;

namespace ChoreBuddies.Backend.Features.Notifications.Email;

public class EmailServiceOptions
{
    public string From { get; set; } = default!;
    public string FromName { get; set; } = default!;
}

public interface IEmailService
{
    public Task<string> SendRegisterConfirmationNotificationAsync(
        AppUser recipientEmail,
        string recipientName,
        CancellationToken cancellationToken = default);
}

public class EmailService : INotificationChannel, IEmailService
{
    NotificationChannel INotificationChannel.ChannelType => NotificationChannel.Email;

    private readonly IMailerooClient _client;
    private readonly string _defaultFrom;
    private readonly string _defaultFromName;

    public EmailService(IMailerooClient mailerooClient, IOptions<EmailServiceOptions> options)
    {
        _client = mailerooClient;
        _defaultFrom = options.Value.From;
        _defaultFromName = options.Value.FromName;
    }
    private static string GetRecipientName(AppUser user)
    {
        return user.UserName ?? user.FirstName ?? "Unknown";
    }
    private async Task<string> SendTemplatedEmailAsync(
        AppUser recipient,
        string toName,
        string templateId,
        string subject,
        Dictionary<string, object> parameters,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateId);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipient.Email);

        var from = new EmailAddress(_defaultFrom, _defaultFromName);
        var to = new EmailAddress(recipient.Email, toName);

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

    public async Task<string> SendNewChoreNotificationAsync(AppUser recipient, int choreId, string choreName, string choreDescription, DateTime? dueDate, CancellationToken cancellationToken = default)
    {
        var recipientName = GetRecipientName(recipient);

        var parameters = new Dictionary<string, object>
        {
            { "choreId", choreId },
            { "choreName", choreName },
            { "choreDescription", choreDescription},
            { "dueDate", dueDate?.ToString("f") ?? "No due date" },
            { "recipientName", recipientName}
        };

        return await SendTemplatedEmailAsync(
            recipient,
            recipientName,
            MailerooConstants.NewChoreTemplate,
            MailSubjects.NewChore,
            parameters,
            cancellationToken);
    }

    public async Task<string> SendRegisterConfirmationNotificationAsync(AppUser recipient, string recipientName, CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, object>
    {
        { "recipientName", recipientName }
    };
        return await SendTemplatedEmailAsync(
            recipient,
            recipientName,
            MailerooConstants.RegisterConfirmationTemplate,
            MailSubjects.RegisterConfirmation,
            parameters,
            cancellationToken);
    }

    public async Task<string> SendNewRewardRequestNotificationAsync(AppUser recipient, int rewardId, string rewardName, string requester, CancellationToken cancellationToken = default)
    {
        var recipientName = GetRecipientName(recipient);

        var parameters = new Dictionary<string, object>
        {
            { "rewardId", rewardId },
            { "recipientName",recipientName},
            { "rewardName", rewardName },
            { "requester", requester }
        };
        return await SendTemplatedEmailAsync(
            recipient,
            recipientName,
            MailerooConstants.NewRewardRequestTemplate,
            MailSubjects.NewRewardRequest,
            parameters,
            cancellationToken);
    }

    public async Task<string> SendNewMessageNotificationAsync(AppUser recipient, string sender, string content, CancellationToken cancellationToken = default)
    {
        var recipientName = GetRecipientName(recipient);

        var parameters = new Dictionary<string, object>
        {
            { "recipientName", GetRecipientName(recipient)},
            { "sender", sender },
            { "content", content }
        };
        return await SendTemplatedEmailAsync(
            recipient,
            recipient.UserName ?? recipient.FirstName ?? "Unknown",
            MailerooConstants.NewMessageTemplate,
            MailSubjects.NewMessage,
            parameters,
            cancellationToken);
    }

    public async Task<string> SendReminderNotificationAsync(AppUser recipient, int choreId, string choreName, CancellationToken cancellationToken = default)
    {
        var recipientName = GetRecipientName(recipient);

        var parameters = new Dictionary<string, object>
        {
            { "choreId", choreId },
            { "recipientName",recipientName},
            { "choreName", choreName }
        };
        return await SendTemplatedEmailAsync(
            recipient,
            recipientName,
            MailerooConstants.ReminderTemplate,
            MailSubjects.Reminder,
            parameters,
            cancellationToken);
    }
}
