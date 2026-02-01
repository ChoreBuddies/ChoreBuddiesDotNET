using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Households;
using ChoreBuddies.Backend.Features.Households.Exceptions;
using ChoreBuddies.Backend.Features.Users;
using FluentAssertions;
using Moq;
using Shared.Households;

namespace ChoreBuddies.Tests.Households;

public class HouseholdsServiceTests
{
    private readonly Mock<IHouseholdRepository> _householdRepo;
    private readonly Mock<IAppUserService> _userService;
    private readonly Mock<IInvitationCodeService> _invitationService;
    private readonly HouseholdService _service;

    public HouseholdsServiceTests()
    {
        _householdRepo = new Mock<IHouseholdRepository>();
        _userService = new Mock<IAppUserService>();
        _invitationService = new Mock<IInvitationCodeService>();

        _service = new HouseholdService(
            _householdRepo.Object,
            _invitationService.Object,
            _userService.Object
        );
    }

    // -----------------------------
    // CreateHouseholdAsync tests
    // -----------------------------
    [Fact]
    public async Task CreateHouseholdAsync_ShouldCreateAndJoinHousehold()
    {
        // Arrange
        _invitationService.Setup(x => x.GenerateUniqueInvitationCodeAsync())
            .ReturnsAsync("INV123");

        var created = new Household(1, "Test", "INV123", "");
        _householdRepo.Setup(x => x.CreateHouseholdAsync(It.IsAny<Household>()))
            .ReturnsAsync(created);

        _userService.Setup(x => x.GetUserByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new AppUser { Id = 10 });

        _householdRepo.Setup(x => x.GetHouseholdByInvitationCodeAsync("INV123"))
            .ReturnsAsync(created);

        // Act
        var result = await _service.CreateHouseholdAsync(
            new CreateHouseholdDto("Test", ""),
            10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("INV123", result.InvitationCode);

        _invitationService.Verify(x => x.GenerateUniqueInvitationCodeAsync(), Times.Once);
        _householdRepo.Verify(x => x.CreateHouseholdAsync(It.IsAny<Household>()), Times.Once);
    }

    [Fact]
    public async Task CreateHouseholdAsync_ShouldThrow_WhenJoinFails()
    {
        // Arrange
        _invitationService.Setup(x => x.GenerateUniqueInvitationCodeAsync())
            .ReturnsAsync("INV123");

        _householdRepo.Setup(x => x.CreateHouseholdAsync(It.IsAny<Household>()))
            .ReturnsAsync(new Household(1, "Test", "INV123", ""));

        _userService.Setup(x => x.GetUserByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((AppUser?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateHouseholdAsync(
                new CreateHouseholdDto("Test", ""), 10));
    }

    // -----------------------------
    // GetUsersHouseholdAsync tests
    // -----------------------------
    [Fact]
    public async Task GetUsersHouseholdAsync_ShouldReturnHousehold()
    {
        // Arrange
        var household = new Household(99, "Test", "INV123", "");
        _householdRepo.Setup(x => x.GetHouseholdByIdAsync(household.Id))
            .ReturnsAsync(household);

        // Act
        var result = await _service.GetUsersHouseholdAsync(household.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(household.Id, result.Id);
    }

    [Fact]
    public async Task GetUsersHouseholdAsync_ShouldReturnNull()
    {
        // Arrange
        _householdRepo.Setup(x => x.GetHouseholdByIdAsync(1))
            .ReturnsAsync((Household?)null);

        // Act
        var result = await _service.GetUsersHouseholdAsync(1);

        // Assert
        Assert.Null(result);
    }

    // -----------------------------
    // JoinHouseholdAsync tests
    // -----------------------------
    [Fact]
    public async Task JoinHouseholdAsync_ShouldJoinUser()
    {
        // Arrange
        var user = new AppUser { Id = 10 };
        var household = new Household(77, "Test", "INV123", "");

        _userService.Setup(x => x.GetUserByIdAsync(10))
            .ReturnsAsync(user);

        _householdRepo.Setup(x => x.GetHouseholdByInvitationCodeAsync("INV123"))
            .ReturnsAsync(household);

        // Act
        var result = await _service.JoinHouseholdAsync("INV123", 10);

        // Assert
        Assert.Equal(household, result);
        _householdRepo.Verify(x => x.JoinHouseholdAsync(household, user), Times.Once);
    }

    [Fact]
    public async Task JoinHouseholdAsync_ShouldThrow_WhenUserNotFound()
    {
        // Arrange
        _userService.Setup(x => x.GetUserByIdAsync(10))
            .ReturnsAsync((AppUser?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.JoinHouseholdAsync("INV123", 10));
    }

    [Fact]
    public async Task JoinHouseholdAsync_ShouldThrow_WhenHouseholdNotFound()
    {
        // Arrange
        _userService.Setup(x => x.GetUserByIdAsync(10))
            .ReturnsAsync(new AppUser { Id = 10 });

        _householdRepo.Setup(x => x.GetHouseholdByInvitationCodeAsync("INV123"))
            .ReturnsAsync((Household?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidInvitationCodeException>(() =>
            _service.JoinHouseholdAsync("INV123", 10));
    }

    // -----------------------------
    // UpdateHouseholdAsync tests
    // -----------------------------
    [Fact]
    public async Task UpdateHouseholdAsync_ShouldUpdate()
    {
        // Arrange
        var household = new Household(3, "Test", "INV123", "");
        _householdRepo.Setup(x => x.GetHouseholdByIdAsync(household.Id))
            .ReturnsAsync(household);

        _householdRepo
            .Setup(x => x.UpdateHouseholdAsync(household, "NewName", "NewDesc"))
            .ReturnsAsync(new Household(3, "NewName", "INV123", description: "NewDesc"));

        var dto = new CreateHouseholdDto("NewName", "NewDesc");

        // Act
        var result = await _service.UpdateHouseholdAsync(household.Id, dto);

        // Assert
        Assert.Equal("NewName", result!.Name);
        Assert.Equal("NewDesc", result.Description);
    }

    [Fact]
    public async Task UpdateHouseholdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        _householdRepo.Setup(x => x.GetHouseholdByIdAsync(3))
            .ReturnsAsync((Household?)null);

        // Act
        var result = await _service.UpdateHouseholdAsync(3, new CreateHouseholdDto("", null));

        // Assert
        Assert.Null(result);
    }

    // -----------------------------
    // DeleteHouseholdAsync tests
    // -----------------------------
    [Fact]
    public async Task DeleteHouseholdAsync_ShouldDelete()
    {
        // Arrange
        var household = new Household(5, "Test", "INV123", "");
        _householdRepo.Setup(x => x.GetHouseholdByIdAsync(household.Id))
            .ReturnsAsync(household);

        _householdRepo.Setup(x => x.DeleteHouseholdAsync(household))
            .ReturnsAsync(household);

        // Act
        var result = await _service.DeleteHouseholdAsync(household.Id);

        // Assert
        Assert.Equal(household.Id, result!.Id);
    }

    [Fact]
    public async Task DeleteHouseholdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        _householdRepo.Setup(x => x.GetHouseholdByIdAsync(5))
            .ReturnsAsync((Household?)null);

        // Act
        var result = await _service.DeleteHouseholdAsync(5);

        // Assert
        Assert.Null(result);
    }

    // -----------------------------
    // CheckIfUserBelongsAsync tests
    // -----------------------------
    [Fact]
    public async Task CheckIfUserBelongsAsync_ShouldReturnValue()
    {
        // Arrange
        _householdRepo.Setup(x => x.CheckIfUserBelongsAsync(1, 10))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CheckIfUserBelongsAsync(1, 10);

        // Assert
        Assert.True(result);
    }

    // -----------------------------
    // GetUserIdForAutoAssignAsync tests
    // -----------------------------
    private static Chore CreateChore(
        int assignedUserId = 1,
        Shared.Chores.Status status = Shared.Chores.Status.Assigned)
    {
        return new Chore(
            name: "Test chore",
            description: "Test description",
            householdId: 1,
            userId: assignedUserId,
            dueDate: null,
            status: status,
            room: "Test",
            rewardPointsCount: 10,
            completedDate: null
        );
    }

    [Fact]
    public async Task GetUserIdForAutoAssignAsync_ShouldReturnLeastBurdenedUserId()
    {
        // Arrange
        var user1 = new AppUser
        {
            Id = 1,
            Chores = new List<Chore>
            {
                CreateChore(),
                CreateChore()
            }
        };

        var user2 = new AppUser
        {
            Id = 2,
            Chores = new List<Chore>
            {
                CreateChore()
            }
        };

        var household = new Household(user1.Id, "", "", "");
        household.Users = new List<AppUser> { user1, user2 };

        _householdRepo
            .Setup(r => r.GeHouseholdWithUsersWithChoresDueToFromDateAsync(
                It.IsAny<int>(),
                It.IsAny<DateTime>()))
            .ReturnsAsync(household);

        // Act
        var result = await _service.GetUserIdForAutoAssignAsync(123);

        // Assert
        result.Should().Be(2);
    }
    [Fact]
    public async Task GetUserIdForAutoAssignAsync_ShouldThrow_WhenUsersAreNull()
    {
        // Arrange
        var user1 = new AppUser
        {
            Id = 1,
            Chores = new List<Chore>
            {
                CreateChore(),
                CreateChore()
            }
        };

        var household = new Household(user1.Id, "", "", "");
        household.Users = [];

        _householdRepo
            .Setup(r => r.GeHouseholdWithUsersWithChoresDueToFromDateAsync(
                It.IsAny<int>(),
                It.IsAny<DateTime>()))
            .ReturnsAsync(household);

        // Act
        Func<Task> act = async () => await _service.GetUserIdForAutoAssignAsync(123);

        // Assert
        await act.Should().ThrowAsync<HouseholdHasNoUsersException>();
    }

    [Fact]
    public async Task GetUserIdForAutoAssignAsync_ShouldThrow_WhenHouseholdIsNull()
    {
        // Arrange
        _householdRepo
            .Setup(r => r.GeHouseholdWithUsersWithChoresDueToFromDateAsync(
                It.IsAny<int>(),
                It.IsAny<DateTime>()))
            .ReturnsAsync((Household?)null);

        // Act
        Func<Task> act = async () => await _service.GetUserIdForAutoAssignAsync(123);

        // Assert
        await act.Should().ThrowAsync<HouseholdDoesNotExistException>();
    }

}
