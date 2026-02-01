namespace ChoreBuddies.Tests.Notifications;

using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Notifications.Exceptions;
using ChoreBuddies.Backend.Features.Notifications.Push;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using FluentAssertions;
using Moq;
using Shared.Notifications;
using Xunit;

public class FirebaseNotificationsServiceTests
{
    private readonly Mock<IFirebaseMessagingClient> _firebaseMock;
    private readonly FirebaseNotificationsService _service;

    public FirebaseNotificationsServiceTests()
    {
        _firebaseMock = new Mock<IFirebaseMessagingClient>();
        _service = new FirebaseNotificationsService(_firebaseMock.Object);
    }

    [Fact]
    public void ChannelType_ShouldBePush()
    {
        _service.ChannelType.Should().Be(NotificationChannel.Push);
    }

    [Fact]
    public async Task SendNewChoreNotificationAsync_NoToken_ReturnsMessage()
    {
        // Arrange
        var user = new AppUser { FcmToken = null };

        // Act
        var result = await _service.SendNewChoreNotificationAsync(
            user,
            choreId: 1,
            choreName: "Dishes",
            choreDescription: "Clean them",
            dueDate: null);

        // Assert
        result.Should().Be("No token present");
        _firebaseMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SendNewChoreNotificationAsync_ValidUser_SendsFirebaseMessage()
    {
        // Arrange
        var user = new AppUser { FcmToken = "fcm-token" };

        _firebaseMock
            .Setup(f => f.SendAsync(It.IsAny<Message>(), default))
            .ReturnsAsync("firebase-response");

        // Act
        var result = await _service.SendNewChoreNotificationAsync(
            user,
            choreId: 42,
            choreName: "Laundry",
            choreDescription: "Wash clothes",
            dueDate: new DateTime(2026, 2, 1));

        // Assert
        result.Should().Be("firebase-response");

        _firebaseMock.Verify(f =>
            f.SendAsync(It.Is<Message>(m =>
                m.Token == "fcm-token" &&
                m.Notification.Title == "New Chore Available!" &&
                m.Data["id"] == "42" &&
                m.Data["screen"] == "/chore_details"
            ), default),
            Times.Once);
    }

    [Fact]
    public async Task SendNewMessageNotificationAsync_SendsCorrectPayload()
    {
        var user = new AppUser { FcmToken = "token" };

        _firebaseMock
            .Setup(f => f.SendAsync(It.IsAny<Message>(), default))
            .ReturnsAsync("ok");

        var result = await _service.SendNewMessageNotificationAsync(
            user,
            sender: "Alice",
            content: "Hello!");

        result.Should().Be("ok");

        _firebaseMock.Verify(f =>
            f.SendAsync(It.Is<Message>(m =>
                m.Notification.Title == "New message from Alice" &&
                m.Notification.Body == "Hello!" &&
                m.Data["screen"] == "/chat"
            ), default),
            Times.Once);
    }

    [Fact]
    public async Task SendNotificationAsync_UnregisteredToken_ThrowsDomainException()
    {
        var user = new AppUser { Id = 123, FcmToken = "dead-token" };

        _firebaseMock
            .Setup(f => f.SendAsync(It.IsAny<Message>(), default))
            .ThrowsAsync(new FirebaseMessagingException(
                ErrorCode.NotFound,
                "Not found",
                MessagingErrorCode.Unregistered));

        Func<Task> act = () => _service.SendNewMessageNotificationAsync(
            user,
            sender: "Alice",
            content: "Hello");

        await act.Should().ThrowAsync<FcmTokenUnregisteredException>()
            .WithMessage("*FCM token*");
    }

}
