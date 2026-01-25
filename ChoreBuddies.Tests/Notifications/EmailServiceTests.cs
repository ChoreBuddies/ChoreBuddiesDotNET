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

        _mailerooClientMock
            .Setup(c => c.SendTemplatedEmailAsync(
                It.IsAny<Dictionary<string, object?>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("reference-id");

        // Act
        var result = await _service.SendRegisterConfirmationNotificationAsync(
            user,
            "John");

        // Assert
        result.Should().Be("reference-id");

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
}
