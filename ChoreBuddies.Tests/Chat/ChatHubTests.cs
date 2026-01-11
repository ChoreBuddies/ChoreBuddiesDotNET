using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Chat;
using ChoreBuddies.Backend.Features.Notifications;
using ChoreBuddies.Backend.Features.Users;
using ChoreBuddies.Backend.Infrastructure.Authentication;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shared.Chat;
using Shared.Households;
using System.Security.Claims;

namespace ChoreBuddies.Tests.Chat;

public class ChatHubTests
{
    // Mocks for dependencies
    private readonly Mock<IChatService> _chatServiceMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IAppUserService> _userServiceMock;
    private readonly Mock<IHouseholdService> _householdServiceMock;
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<INotificationService> _notificationServiceMock;

    // Mocks for SignalR context
    private readonly Mock<IHubCallerClients> _clientsMock;
    private readonly Mock<IClientProxy> _clientProxyMock;
    private readonly Mock<ISingleClientProxy> _callerProxyMock;
    private readonly Mock<HubCallerContext> _contextMock;
    private readonly Mock<IGroupManager> _groupsMock;

    private readonly ChatHub _chatHub;

    public ChatHubTests()
    {
        // 1. Init Mocks
        _chatServiceMock = new Mock<IChatService>();
        _tokenServiceMock = new Mock<ITokenService>();
        _userServiceMock = new Mock<IAppUserService>();
        _householdServiceMock = new Mock<IHouseholdService>();
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _notificationServiceMock = new Mock<INotificationService>();

        _clientsMock = new Mock<IHubCallerClients>();
        _clientProxyMock = new Mock<IClientProxy>();
        _callerProxyMock = new Mock<ISingleClientProxy>();
        _contextMock = new Mock<HubCallerContext>();
        _groupsMock = new Mock<IGroupManager>();

        // 2. Setup ScopeFactory 
        var scopeMock = new Mock<IServiceScope>();
        var serviceProviderMock = new Mock<IServiceProvider>();

        scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);
        _scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

        serviceProviderMock.Setup(sp => sp.GetService(typeof(IAppUserService)))
            .Returns(_userServiceMock.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(INotificationService)))
            .Returns(_notificationServiceMock.Object);

        // 3. Setup SignalR Basics
        _clientsMock.Setup(c => c.Caller).Returns(_callerProxyMock.Object);
        _clientsMock.Setup(c => c.OthersInGroup(It.IsAny<string>())).Returns(_clientProxyMock.Object);
        _clientsMock.Setup(c => c.GroupExcept(It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>()))
             .Returns(_clientProxyMock.Object);

        // 4. Create System Under Test (SUT)
        _chatHub = new ChatHub(
            _chatServiceMock.Object,
            _tokenServiceMock.Object,
            _userServiceMock.Object,
            _householdServiceMock.Object,
            _scopeFactoryMock.Object
        )
        {
            Clients = _clientsMock.Object,
            Context = _contextMock.Object,
            Groups = _groupsMock.Object
        };
    }

    // --- JOIN CHAT TESTS ---

    [Fact]
    public async Task JoinHouseholdChat_UserHasAccess_AddsToGroup()
    {
        // Arrange
        int householdId = 5;
        int userId = 10;
        string connectionId = "conn_123";
        string groupName = "Household_5";

        _tokenServiceMock.Setup(x => x.GetUserIdFromToken(It.IsAny<ClaimsPrincipal>())).Returns(userId);
        _householdServiceMock.Setup(x => x.CheckIfUserBelongsAsync(householdId, userId))
            .ReturnsAsync(true);
        _chatServiceMock.Setup(x => x.GetGroupName(householdId)).Returns(groupName);
        _contextMock.Setup(x => x.ConnectionId).Returns(connectionId);

        // Act
        await _chatHub.JoinHouseholdChat(householdId);

        // Assert
        _groupsMock.Verify(x => x.AddToGroupAsync(connectionId, groupName, default), Times.Once);
    }

    [Fact]
    public async Task JoinHouseholdChat_UserNoAccess_ThrowsHubException()
    {
        // Arrange
        int householdId = 5;
        int userId = 99;

        _tokenServiceMock.Setup(x => x.GetUserIdFromToken(It.IsAny<ClaimsPrincipal>())).Returns(userId);
        _householdServiceMock.Setup(x => x.CheckIfUserBelongsAsync(householdId, userId))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<HubException>(() => _chatHub.JoinHouseholdChat(householdId));

        _groupsMock.Verify(x => x.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
    }

    // --- SEND MESSAGE TESTS ---

    [Fact]
    public async Task SendMessage_ValidRequest_BroadcastsMessages()
    {
        // Arrange
        int householdId = 1;
        int userId = 10;
        string userName = "TestUser";
        var user = new AppUser() { Id = userId, HouseholdId = householdId, UserName = userName };
        string content = "Hello World";
        Guid clientUniqueId = Guid.NewGuid();
        string groupName = "Household_1";

        // Mock User & Auth
        _tokenServiceMock.Setup(x => x.GetUserIdFromToken(It.IsAny<ClaimsPrincipal>())).Returns(userId);
        _tokenServiceMock.Setup(x => x.GetUserNameFromToken(It.IsAny<ClaimsPrincipal>())).Returns(userName);
        _userServiceMock.Setup(x => x.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(user);
        _householdServiceMock.Setup(x => x.CheckIfUserBelongsAsync(householdId, userId)).ReturnsAsync(true);
        _chatServiceMock.Setup(x => x.GetGroupName(householdId)).Returns(groupName);

        // Mock Message Creation Success
        var createdMessageDto = new ChatMessageDto(100, userName, content, DateTimeOffset.UtcNow, false, clientUniqueId);
        _chatServiceMock.Setup(x => x.CreateChatMessageAsync(user, content, clientUniqueId))
            .ReturnsAsync(createdMessageDto);

        // Act
        await _chatHub.SendMessage(householdId, content, clientUniqueId);

        // Assert
        _clientsMock.Verify(c => c.OthersInGroup(groupName), Times.Once);
        _clientProxyMock.Verify(p => p.SendCoreAsync(
            ChatConstants.ReceiveMessage,
            It.Is<object[]>(args =>
                args.Length == 1 &&
                ((ChatMessageDto)args[0]).IsMine == false &&
                ((ChatMessageDto)args[0]).Content == content
            ),
            default), Times.Once);

        _clientsMock.Verify(c => c.Caller, Times.Once);
        _callerProxyMock.Verify(p => p.SendCoreAsync(
            ChatConstants.ReceiveMessage,
            It.Is<object[]>(args =>
                args.Length == 1 &&
                ((ChatMessageDto)args[0]).IsMine == true
            ),
            default), Times.Once);
    }

    [Fact]
    public async Task SendMessage_ServiceReturnsNull_ThrowsHubException()
    {
        // Arrange
        int householdId = 1;
        int userId = 10;

        _tokenServiceMock.Setup(x => x.GetUserIdFromToken(It.IsAny<ClaimsPrincipal>())).Returns(userId);
        _householdServiceMock.Setup(x => x.CheckIfUserBelongsAsync(householdId, userId)).ReturnsAsync(true);
        _tokenServiceMock.Setup(x => x.GetUserNameFromToken(It.IsAny<ClaimsPrincipal>())).Returns("User");

        _chatServiceMock.Setup(x => x.CreateChatMessageAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
            .ReturnsAsync((ChatMessageDto?)null);

        // Act & Assert
        await Assert.ThrowsAsync<HubException>(() =>
            _chatHub.SendMessage(householdId, "fail", Guid.NewGuid()));
    }

    [Fact]
    public async Task SendMessage_UserNoAccess_ThrowsHubException()
    {
        // Arrange
        _tokenServiceMock.Setup(x => x.GetUserIdFromToken(It.IsAny<ClaimsPrincipal>())).Returns(1);
        _householdServiceMock.Setup(x => x.CheckIfUserBelongsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<HubException>(() =>
            _chatHub.SendMessage(1, "msg", Guid.NewGuid()));

        _chatServiceMock.Verify(x => x.CreateChatMessageAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
    }

    // --- TYPING INDICATOR TESTS ---

    [Fact]
    public async Task SendTyping_ValidRequest_BroadcastsToOthers()
    {
        // Arrange
        int householdId = 10;
        string userName = "Typer";
        string connectionId = "conn_type";
        string groupName = "Household_10";

        _tokenServiceMock.Setup(x => x.GetUserNameFromToken(It.IsAny<ClaimsPrincipal>())).Returns(userName);
        _chatServiceMock.Setup(x => x.GetGroupName(householdId)).Returns(groupName);
        _contextMock.Setup(x => x.ConnectionId).Returns(connectionId);

        // Act
        await _chatHub.SendTyping(householdId);

        // Assert
        _clientsMock.Verify(c => c.GroupExcept(
            groupName,
            It.Is<IReadOnlyList<string>>(list => list.Contains(connectionId))
        ), Times.Once);

        _clientProxyMock.Verify(p => p.SendCoreAsync(
            ChatConstants.UserIsTyping,
            It.Is<object[]>(args => (string)args[0] == userName),
            default), Times.Once);
    }
}
