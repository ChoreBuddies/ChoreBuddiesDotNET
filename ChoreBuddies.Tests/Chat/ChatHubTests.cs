using ChoreBuddies.Backend.Features.Chat;
using ChoreBuddies.Backend.Features.Households;
using ChoreBuddies.Backend.Features.Users;
using ChoreBuddies.Backend.Infrastructure.Authentication;
using ChoreBuddies.Tests.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Security.Claims;

namespace ChoreBuddies.Tests.Chat;

public class ChatHubTests : BaseIntegrationTest
{
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IAppUserService> _userServiceMock;
    private readonly Mock<IHouseholdService> _householdServiceMock;
    private readonly Mock<TimeProvider> _timeProviderMock;
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;

    // Mocki SignalR
    private readonly Mock<IHubCallerClients> _clientsMock;
    private readonly Mock<IClientProxy> _clientProxyMock;
    private readonly Mock<HubCallerContext> _contextMock;
    private readonly ChatHub _chatHub;

    public ChatHubTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        _tokenServiceMock = new Mock<ITokenService>();
        _userServiceMock = new Mock<IAppUserService>();
        _householdServiceMock = new Mock<IHouseholdService>();
        _timeProviderMock = new Mock<TimeProvider>();
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();

        _clientsMock = new Mock<IHubCallerClients>();
        _clientProxyMock = new Mock<IClientProxy>();
        _contextMock = new Mock<HubCallerContext>();

        _chatHub = new ChatHub(
            DbContext,
            _tokenServiceMock.Object,
            _userServiceMock.Object,
            _householdServiceMock.Object,
            _scopeFactoryMock.Object,
            _timeProviderMock.Object
        )
        {
            Clients = _clientsMock.Object,
            Context = _contextMock.Object
        };
    }

    [Fact]
    public async Task SendMessage_UserNotInHousehold_ThrowsHubException()
    {
        // Arrange
        int householdId = 1;
        int userId = 100;

        _tokenServiceMock.Setup(x => x.GetUserIdFromToken(It.IsAny<ClaimsPrincipal>())).Returns(userId);
        _householdServiceMock.Setup(x => x.CheckIfUserBelongsAsync(householdId, userId))
            .ReturnsAsync(false); // User nie należy do domu

        // Act & Assert
        await Assert.ThrowsAsync<HubException>(() =>
            _chatHub.SendMessage(householdId, "Hello", Guid.NewGuid()));
    }

    [Fact]
    public async Task JoinHouseholdChat_UserHasAccess_AddsToGroup()
    {
        // Arrange
        int householdId = 5;
        int userId = 10;
        string connectionId = "conn1";

        _tokenServiceMock.Setup(x => x.GetUserIdFromToken(It.IsAny<ClaimsPrincipal>())).Returns(userId);
        _householdServiceMock.Setup(x => x.CheckIfUserBelongsAsync(householdId, userId))
            .ReturnsAsync(true);
        _contextMock.Setup(x => x.ConnectionId).Returns(connectionId);

        var groupsMock = new Mock<IGroupManager>();
        _chatHub.Groups = groupsMock.Object;

        // Act
        await _chatHub.JoinHouseholdChat(householdId);

        // Assert
        // Sprawdzamy czy metoda AddToGroupAsync została wywołana z poprawną nazwą grupy
        groupsMock.Verify(x => x.AddToGroupAsync(connectionId, $"Household_{householdId}", default), Times.Once);
    }
}
