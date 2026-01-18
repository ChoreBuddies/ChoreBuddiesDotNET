using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Notifications;
using ChoreBuddies.Backend.Features.Notifications.NotificationPreferences;
using ChoreBuddies.Backend.Features.Users;
using FluentAssertions;
using Moq;
using Shared.Notifications;

namespace ChoreBuddies.Tests.Notifications;

public class NotificationServiceTests
{
    private readonly Mock<IAppUserService> _userService = new();
    private readonly Mock<INotificationPreferenceService> _notificationPreverenceService = new();

    private readonly Mock<INotificationChannel> _emailChannelMock = new();
    private readonly Mock<INotificationChannel> _pushChannelMock = new();
    private readonly NotificationService _service;
    private readonly AppUser _recipient;

    public NotificationServiceTests()
    {
        _recipient = new AppUser
        {
            Id = 1,
            Email = "test@test.com"
        };

        _emailChannelMock.SetupGet(c => c.ChannelType)
            .Returns(NotificationChannel.Email);

        _pushChannelMock.SetupGet(c => c.ChannelType)
            .Returns(NotificationChannel.Push);

        _userService
            .Setup(x => x.GetUserByIdAsync(_recipient.Id))
            .ReturnsAsync(_recipient);

        _service = new NotificationService(
            [_emailChannelMock.Object, _pushChannelMock.Object],
            _notificationPreverenceService.Object,
            _userService.Object);
    }
    [Fact]
    public async Task SendNewChoreNotificationAsync_ShouldThrow_WhenRecipientDoesNotExist()
    {
        _userService
            .Setup(x => x.GetUserByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((AppUser?)null);

        var act = async () =>
            await _service.SendNewChoreNotificationAsync(
                999, -1, "Chore", "Description", DateTime.UtcNow);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid recipient");
    }

    [Fact]
    public async Task SendNewChoreNotificationAsync_ShouldSendOnlyToPreferredChannels()
    {
        _notificationPreverenceService
            .Setup(x => x.GetActiveChannelsAsync(
                _recipient,
                NotificationEvent.NewChore,
                It.IsAny<CancellationToken>())).ReturnsAsync([NotificationChannel.Email]);

        _emailChannelMock
            .Setup(x => x.SendNewChoreNotificationAsync(
                _recipient,
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("id");

        var result = await _service.SendNewChoreNotificationAsync(
            _recipient.Id,
            1,
            "Wash dishes",
            "Kitchen sink",
            DateTime.UtcNow);

        result.Should().BeTrue();

        _emailChannelMock.Verify(x =>
            x.SendNewChoreNotificationAsync(
                _recipient,
                1,
                "Wash dishes",
                "Kitchen sink",
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _pushChannelMock.Verify(x =>
            x.SendNewChoreNotificationAsync(
                It.IsAny<AppUser>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Fact]
    public async Task SendNewMessageNotificationAsync_ShouldSendToAllEnabledChannels()
    {
        _notificationPreverenceService
            .Setup(x => x.GetActiveChannelsAsync(
                _recipient,
                NotificationEvent.NewMessage,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                NotificationChannel.Email,
                NotificationChannel.Push
            ]);

        _emailChannelMock
            .Setup(x => x.SendNewMessageNotificationAsync(
                It.IsAny<AppUser>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("id");

        _pushChannelMock
            .Setup(x => x.SendNewMessageNotificationAsync(
                It.IsAny<AppUser>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("id");

        var result = await _service.SendNewMessageNotificationAsync(
            _recipient.Id,
            "Alice",
            "Hello!");

        result.Should().BeTrue();

        _emailChannelMock.Verify(x =>
            x.SendNewMessageNotificationAsync(
                _recipient,
                "Alice",
                "Hello!",
                It.IsAny<CancellationToken>()),
            Times.Once);

        _pushChannelMock.Verify(x =>
            x.SendNewMessageNotificationAsync(
                _recipient,
                "Alice",
                "Hello!",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task SendReminderAsync_ShouldUseReminderEvent()
    {
        _notificationPreverenceService
            .Setup(x => x.GetActiveChannelsAsync(
                _recipient,
                NotificationEvent.Reminder,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([NotificationChannel.Push]);

        _pushChannelMock
            .Setup(x => x.SendReminderNotificationAsync(
                _recipient,
                It.IsAny<int>(),
                "Take out trash",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("id");

        var result = await _service.SendReminderAsync(
            _recipient.Id,
            -1,
            "Take out trash");

        result.Should().BeTrue();

        _pushChannelMock.Verify(x =>
            x.SendReminderNotificationAsync(
                _recipient,
                -1,
                "Take out trash",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
