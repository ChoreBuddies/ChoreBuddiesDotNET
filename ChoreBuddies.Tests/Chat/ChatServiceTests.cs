using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Chat;
using FluentAssertions;
using Moq;

namespace ChoreBuddies.Tests.Chat;

public class ChatServiceTests
{
    private readonly Mock<IChatRepository> _repoMock;
    private readonly Mock<TimeProvider> _timeProviderMock;
    private readonly ChatService _service;

    public ChatServiceTests()
    {
        _repoMock = new Mock<IChatRepository>();
        _timeProviderMock = new Mock<TimeProvider>();

        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero));

        _service = new ChatService(_repoMock.Object, _timeProviderMock.Object);
    }

    // ---------------------------
    // GetGroupName
    // ---------------------------
    [Fact]
    public void GetGroupName_ShouldReturnFormattedString()
    {
        // Act
        var result = _service.GetGroupName(123);

        // Assert
        result.Should().Be("Household_123");
    }

    // ---------------------------
    // CreateChatMessageAsync (Overload with specific params)
    // ---------------------------
    [Fact]
    public async Task CreateChatMessageAsync_ShouldCreateEntityAndReturnDto()
    {
        // Arrange
        int userId = 10;
        int householdId = 5;
        string userName = "TestUser";
        string content = "Hello World";
        Guid clientUniqueId = Guid.NewGuid();
        var fixedTime = _timeProviderMock.Object.GetUtcNow();

        var createdEntity = new ChatMessage(userId, householdId, content, fixedTime)
        {
            Id = 100
        };

        _repoMock.Setup(x => x.CreateChatMessageAsync(It.IsAny<ChatMessage>()))
            .ReturnsAsync(createdEntity);

        // Act
        var result = await _service.CreateChatMessageAsync(userId, householdId, userName, content, clientUniqueId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(100);
        result.SenderName.Should().Be(userName);
        result.Content.Should().Be(content);
        result.SentAt.Should().Be(fixedTime);
        result.ClientUniqueId.Should().Be(clientUniqueId);

        _repoMock.Verify(x => x.CreateChatMessageAsync(It.Is<ChatMessage>(msg =>
            msg.SenderId == userId &&
            msg.HouseholdId == householdId &&
            msg.Content == content &&
            msg.SentAt == fixedTime
        )), Times.Once);
    }

    [Fact]
    public async Task CreateChatMessageAsync_ShouldReturnNull_WhenRepoReturnsNull()
    {
        // Arrange
        _repoMock.Setup(x => x.CreateChatMessageAsync(It.IsAny<ChatMessage>()))
            .ReturnsAsync((ChatMessage?)null);

        // Act
        var result = await _service.CreateChatMessageAsync(1, 1, "User", "Content", Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    // ---------------------------
    // CreateChatMessageAsync (Overload with AppUser)
    // ---------------------------
    [Fact]
    public async Task CreateChatMessageAsync_WithAppUser_ShouldExtractDataAndCallRepo()
    {
        // Arrange
        var user = new AppUser
        {
            Id = 99,
            UserName = "AppUserTest",
            HouseholdId = 77
        };
        string content = "Message from object";
        Guid guid = Guid.NewGuid();
        var fixedTime = _timeProviderMock.Object.GetUtcNow();

        var createdEntity = new ChatMessage(user.Id, user.HouseholdId.Value, content, fixedTime) { Id = 200 };

        _repoMock.Setup(x => x.CreateChatMessageAsync(It.IsAny<ChatMessage>()))
            .ReturnsAsync(createdEntity);

        // Act
        var result = await _service.CreateChatMessageAsync(user, content, guid);

        // Assert
        result.Should().NotBeNull();
        result!.SenderName.Should().Be("AppUserTest");
        result!.Id.Should().Be(200);

        _repoMock.Verify(x => x.CreateChatMessageAsync(It.Is<ChatMessage>(m =>
            m.SenderId == 99 &&
            m.HouseholdId == 77
        )), Times.Once);
    }

    // ---------------------------
    // GetNewestMessagesAsync
    // ---------------------------
    [Fact]
    public async Task GetNewestMessagesAsync_ShouldMapEntitiesToDtos_AndSetIsMineFlag()
    {
        // Arrange
        int currentUserId = 1;
        int otherUserId = 2;
        int householdId = 10;
        int limit = 10;
        var fixedTime = _timeProviderMock.Object.GetUtcNow();

        var messages = new List<ChatMessage>
        {
            new ChatMessage(currentUserId, householdId, "My Message", fixedTime)
            {
                Id = 1,
                Sender = new AppUser { UserName = "Me" }
            },
            new ChatMessage(otherUserId, householdId, "Other Message", fixedTime)
            {
                Id = 2,
                Sender = new AppUser { UserName = "SomeoneElse" }
            }
        };

        _repoMock.Setup(x => x.GetNewestMessagesAsync(householdId, limit))
            .ReturnsAsync(messages);

        // Act
        var result = await _service.GetNewestMessagesAsync(currentUserId, householdId, limit);

        // Assert
        result.Should().HaveCount(2);

        var myMsg = result.Find(m => m.Id == 1);
        myMsg.Should().NotBeNull();
        myMsg!.IsMine.Should().BeTrue();
        myMsg.SenderName.Should().Be("Me");

        var otherMsg = result.Find(m => m.Id == 2);
        otherMsg.Should().NotBeNull();
        otherMsg!.IsMine.Should().BeFalse();
        otherMsg.SenderName.Should().Be("SomeoneElse");
    }

    [Fact]
    public async Task GetNewestMessagesAsync_ShouldHandleNullSender_AsUnknown()
    {
        // Arrange
        int householdId = 10;
        var fixedTime = _timeProviderMock.Object.GetUtcNow();

        var messages = new List<ChatMessage>
        {
            new ChatMessage(999, householdId, "Ghost Message", fixedTime)
            {
                Id = 1,
                Sender = null
            }
        };

        _repoMock.Setup(x => x.GetNewestMessagesAsync(householdId, 50))
            .ReturnsAsync(messages);

        // Act
        var result = await _service.GetNewestMessagesAsync(1, householdId);

        // Assert
        result.Should().HaveCount(1);
        result[0].SenderName.Should().Be("Unknown");
    }

    [Fact]
    public async Task GetNewestMessagesAsync_ShouldHandleNullUserName_AsUnknown()
    {
        // Arrange
        int householdId = 10;

        var messages = new List<ChatMessage>
        {
            new ChatMessage(999, householdId, "NoName Message", DateTimeOffset.UtcNow)
            {
                Id = 1,
                Sender = new AppUser { UserName = null }
            }
        };

        _repoMock.Setup(x => x.GetNewestMessagesAsync(householdId, 50))
            .ReturnsAsync(messages);

        // Act
        var result = await _service.GetNewestMessagesAsync(1, householdId);

        // Assert
        result[0].SenderName.Should().Be("Unknown");
    }
}
