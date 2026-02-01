using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Notifications;
using ChoreBuddies.Backend.Features.Notifications.Email;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Shared.Notifications;

namespace ChoreBuddies.Tests.Notifications;

public class EmailServiceTests
{
    private readonly Mock<IMailerooClient> _mailerooClientMock;
    private readonly EmailService _service;

    public EmailServiceTests()
    {
        _mailerooClientMock = new Mock<IMailerooClient>();

        var options = Options.Create(new EmailServiceOptions
        {
            From = "noreply@test.com",
            FromName = "ChoreBuddies"
        });

        _service = new EmailService(_mailerooClientMock.Object, options);
    }

    [Fact]
    public async Task SendRegisterConfirmationNotificationAsync_ShouldSendTemplatedEmail()
    {
        // Arrange
        var user = new AppUser
        {
            Email = "user@test.com",
            UserName = "John"
        };

        _mailerooClientMock.SetupReturnsReferenceId("ref-id");

        // Act
        var result = await _service.SendRegisterConfirmationNotificationAsync(
            user,
            "John");

        // Assert
        result.Should().Be("ref-id");

        _mailerooClientMock.Verify(c =>
            c.SendTemplatedEmailAsync(
                It.Is<Dictionary<string, object?>>(payload =>
                    payload["template_id"]!.Equals(MailerooConstants.RegisterConfirmationTemplate) &&
                    payload["subject"]!.Equals(MailSubjects.RegisterConfirmation) &&
                    ((Dictionary<string, object>)payload["template_data"]!)["recipientName"]
                        .Equals("John")
                ),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendRegisterConfirmationNotificationAsync_ShouldThrow_WhenEmailIsMissing()
    {
        // Arrange
        var user = new AppUser
        {
            Email = null
        };

        // Act
        var act = async () =>
            await _service.SendRegisterConfirmationNotificationAsync(user, "John");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public void EmailService_ShouldExposeEmailChannelType()
    {
        // Arrange
        var channel = (INotificationChannel)_service;

        // Act
        var result = channel.ChannelType;

        // Assert
        result.Should().Be(NotificationChannel.Email);
    }

    [Fact]
    public async Task SendNewChoreNotificationAsync_ShouldSendCorrectData_WhenDueDateIsPresent()
    {
        // Arrange
        var user = new AppUser { Email = "user@test.com", UserName = "Alice" };
        var dueDate = new DateTime(2025, 12, 24, 15, 30, 0);

        _mailerooClientMock.SetupReturnsReferenceId("ref-chore");

        // Act
        var result = await _service.SendNewChoreNotificationAsync(
            user,
            101,
            "Clean Room",
            "Vacuum and dust",
            dueDate);

        // Assert
        result.Should().Be("ref-chore");

        _mailerooClientMock.Verify(c => c.SendTemplatedEmailAsync(
            It.Is<Dictionary<string, object?>>(payload =>
                payload["template_id"]!.Equals(MailerooConstants.NewChoreTemplate) &&
                payload["subject"]!.Equals(MailSubjects.NewChore) &&
                VerifyTemplateData(payload, data =>
                    data["choreId"].Equals(101) &&
                    data["choreName"].Equals("Clean Room") &&
                    data["choreDescription"].Equals("Vacuum and dust") &&
                    data["recipientName"].Equals("Alice") &&
                    data["dueDate"].Equals(dueDate.ToString("f")) // Sprawdzenie formatowania daty
                )
            ),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendNewChoreNotificationAsync_ShouldHandleNullDueDate()
    {
        // Arrange
        var user = new AppUser { Email = "user@test.com", UserName = "Bob" };

        _mailerooClientMock.SetupReturnsReferenceId();

        // Act
        await _service.SendNewChoreNotificationAsync(user, 102, "Dishes", "Wash them", null);

        // Assert
        _mailerooClientMock.Verify(c => c.SendTemplatedEmailAsync(
            It.Is<Dictionary<string, object?>>(payload =>
                VerifyTemplateData(payload, data =>
                    data["dueDate"].Equals("No due date")
                )
            ),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendNewRewardRequestNotificationAsync_ShouldSendCorrectPayload()
    {
        // Arrange
        var user = new AppUser { Email = "parent@test.com", UserName = "Dad" };
        _mailerooClientMock.SetupReturnsReferenceId();

        // Act
        await _service.SendNewRewardRequestNotificationAsync(user, 5, "Ice Cream", "Kiddo");

        // Assert
        _mailerooClientMock.Verify(c => c.SendTemplatedEmailAsync(
            It.Is<Dictionary<string, object?>>(payload =>
                payload["template_id"]!.Equals(MailerooConstants.NewRewardRequestTemplate) &&
                payload["subject"]!.Equals(MailSubjects.NewRewardRequest) &&
                VerifyTemplateData(payload, data =>
                    data["rewardId"].Equals(5) &&
                    data["rewardName"].Equals("Ice Cream") &&
                    data["requester"].Equals("Kiddo") &&
                    data["recipientName"].Equals("Dad")
                )
            ),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendNewMessageNotificationAsync_ShouldUseFirstName_WhenUserNameIsNull()
    {
        // Arrange
        var user = new AppUser { Email = "user@test.com", UserName = null, FirstName = "Anna" };
        _mailerooClientMock.SetupReturnsReferenceId();

        // Act
        await _service.SendNewMessageNotificationAsync(user, "Admin", "Hello there");

        // Assert
        _mailerooClientMock.Verify(c => c.SendTemplatedEmailAsync(
            It.Is<Dictionary<string, object?>>(payload =>
                payload["template_id"]!.Equals(MailerooConstants.NewMessageTemplate) &&
                payload["subject"]!.Equals(MailSubjects.NewMessage) &&
                VerifyTemplateData(payload, data =>
                    data["recipientName"].Equals("Anna") &&
                    data["sender"].Equals("Admin") &&
                    data["content"].Equals("Hello there")
                )
            ),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendReminderNotificationAsync_ShouldUseUnknown_WhenNamesAreMissing()
    {
        // Arrange
        var user = new AppUser { Email = "ghost@test.com", UserName = null, FirstName = null };
        _mailerooClientMock.SetupReturnsReferenceId();

        // Act
        await _service.SendReminderNotificationAsync(user, 99, "Trash");

        // Assert
        _mailerooClientMock.Verify(c => c.SendTemplatedEmailAsync(
            It.Is<Dictionary<string, object?>>(payload =>
                payload["template_id"]!.Equals(MailerooConstants.ReminderTemplate) &&
                payload["subject"]!.Equals(MailSubjects.Reminder) &&
                VerifyTemplateData(payload, data =>
                    data["recipientName"].Equals("Unknown") &&
                    data["choreName"].Equals("Trash") &&
                    data["choreId"].Equals(99)
                )
            ),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static bool VerifyTemplateData(Dictionary<string, object?> payload, Func<Dictionary<string, object>, bool> verification)
    {
        if (payload.TryGetValue("template_data", out var dataObj) && dataObj is Dictionary<string, object> dataDict)
        {
            return verification(dataDict);
        }
        return false;
    }
}

public static class MailerooMockExtensions
{
    public static void SetupReturnsReferenceId(this Mock<IMailerooClient> mock, string id = "ref-id")
    {
        mock.Setup(c => c.SendTemplatedEmailAsync(It.IsAny<Dictionary<string, object?>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(id);
    }
}

