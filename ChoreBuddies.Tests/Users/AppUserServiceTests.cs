using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Households.Exceptions;
using ChoreBuddies.Backend.Features.Users;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shared.Authentication;
using Shared.Users;

namespace ChoreBuddies.Tests.Users;

public class AppUserServiceTests
{
    private readonly Mock<IAppUserRepository> _userRepositoryMock;
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly Mock<RoleManager<IdentityRole<int>>> _roleManagerMock;
    private readonly AppUserService _service;

    public AppUserServiceTests()
    {
        _userRepositoryMock = new Mock<IAppUserRepository>();

        var userStoreMock = new Mock<IUserStore<AppUser>>();
        _userManagerMock = new Mock<UserManager<AppUser>>(
            userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var roleStoreMock = new Mock<IRoleStore<IdentityRole<int>>>();
        _roleManagerMock = new Mock<RoleManager<IdentityRole<int>>>(
            roleStoreMock.Object, null!, null!, null!, null!);

        _service = new AppUserService(
            _userRepositoryMock.Object,
            _userManagerMock.Object,
            _roleManagerMock.Object);
    }

    // --- GetUser Tests ---

    [Fact]
    public async Task GetUserByEmailAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var email = "test@test.com";
        var user = new AppUser { Email = email };
        _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(email)).ReturnsAsync(user);

        // Act
        var result = await _service.GetUserByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var id = 1;
        var user = new AppUser { Id = id };
        _userRepositoryMock.Setup(x => x.GetUserByIdAsync(id)).ReturnsAsync(user);

        // Act
        var result = await _service.GetUserByIdAsync(id);

        // Assert
        result.Should().BeEquivalentTo(user);
    }

    // --- UpdateUserAsync Tests ---

    [Fact]
    public async Task UpdateUserAsync_ShouldReturnFalse_WhenUserNotFound()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync((AppUser?)null);
        var dto = new UpdateAppUserDto(1, "First", "Last", DateTime.Now, "user", "test@test.com");

        // Act
        var result = await _service.UpdateUserAsync(1, dto);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldUpdateAndReturnTrue_WhenUserExists()
    {
        // Arrange
        var user = new AppUser { Id = 1, FirstName = "Old" };
        var dto = new UpdateAppUserDto(1, "New", "Last", DateTime.Now, "user", "test@test.com");

        _userRepositoryMock.Setup(x => x.GetUserByIdAsync(1)).ReturnsAsync(user);

        // Act
        var result = await _service.UpdateUserAsync(1, dto);

        // Assert
        result.Should().BeTrue();
        user.FirstName.Should().Be("New");
        user.LastName.Should().Be("Last");
        _userRepositoryMock.Verify(x => x.UpdateUserAsync(user), Times.Once);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    // --- Points Tests ---

    [Fact]
    public async Task GetUserPointsCountAsync_ShouldThrow_WhenUserIsNull()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetUserByIdAsync(123)).ReturnsAsync((AppUser?)null);

        // Act
        Func<Task> act = async () => await _service.GetUserPointsCountAsync(123);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User with id 123 not found.");
    }
    [Fact]
    public async Task GetUserPointsCountAsync_ShouldReturnUserPointsCount()
    {
        // Arrange
        var user = new AppUser { Id = 123, FirstName = "user", PointsCount = 100 };
        _userRepositoryMock.Setup(r => r.GetUserByIdAsync(123)).ReturnsAsync(user);

        // Act
        var result = await _service.GetUserPointsCountAsync(123);

        // Assert
        result.Should().Be(100);
    }

    [Fact]
    public async Task AddPointsToUser_ShouldReturnFalse_WhenPointsNegative()
    {
        var result = await _service.AddPointsToUser(1, -5);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddPointsToUser_ShouldAddPoints_WhenValid()
    {
        // Arrange
        var user = new AppUser { Id = 1, PointsCount = 10 };
        _userRepositoryMock.Setup(x => x.GetUserByIdAsync(1)).ReturnsAsync(user);

        // Act
        var result = await _service.AddPointsToUser(1, 5);

        // Assert
        result.Should().BeTrue();
        user.PointsCount.Should().Be(15);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RemovePointsFromUser_ShouldReturnFalse_WhenInsufficientPoints()
    {
        // Arrange
        var user = new AppUser { Id = 1, PointsCount = 5 };
        _userRepositoryMock.Setup(x => x.GetUserByIdAsync(1)).ReturnsAsync(user);

        // Act
        var result = await _service.RemovePointsFromUser(1, 10);

        // Assert
        result.Should().BeFalse();
        user.PointsCount.Should().Be(5);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task RemovePointsFromUser_ShouldRemovePoints_WhenSufficient()
    {
        // Arrange
        var user = new AppUser { Id = 1, PointsCount = 20 };
        _userRepositoryMock.Setup(x => x.GetUserByIdAsync(1)).ReturnsAsync(user);

        // Act
        var result = await _service.RemovePointsFromUser(1, 10);

        // Assert
        result.Should().BeTrue();
        user.PointsCount.Should().Be(10);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    // --- FCM Token Tests ---

    [Fact]
    public async Task UpdateFcmTokenAsync_ShouldUpdateToken_WhenUserExists()
    {
        // Arrange
        var user = new AppUser { Id = 1, FcmToken = "OldToken" };
        var dto = new UpdateFcmTokenDto("NewToken");
        _userRepositoryMock.Setup(x => x.GetUserByIdAsync(1)).ReturnsAsync(user);

        // Act
        var result = await _service.UpdateFcmTokenAsync(1, dto);

        // Assert
        result.Should().BeTrue();
        user.FcmToken.Should().Be("NewToken");
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ClearFcmTokenAsync_ShouldClearToken_WhenUserExists()
    {
        // Arrange
        var user = new AppUser { Id = 1, FcmToken = "Token" };
        _userRepositoryMock.Setup(x => x.GetUserByIdAsync(1)).ReturnsAsync(user);

        // Act
        var result = await _service.ClearFcmTokenAsync(1);

        // Assert
        result.Should().BeTrue();
        user.FcmToken.Should().BeNull();
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    // --- Household Tests ---

    [Fact]
    public async Task GetUsersHouseholdMembersWithRolesAsync_ShouldThrow_WhenNoHousehold()
    {
        // Arrange
        var user = new AppUser { Id = 1, Household = null };
        _userRepositoryMock.Setup(x => x.GetUserWithHouseholdByIdAsync(1)).ReturnsAsync(user);

        // Act
        var act = async () => await _service.GetUsersHouseholdMembersWithRolesAsync(1);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("User doesn't belong to any household");
    }

    [Fact]
    public async Task GetUsersHouseholdMembersWithRolesAsync_ShouldReturnMembersWithRoles()
    {
        // Arrange
        var member1 = new AppUser { Id = 2, UserName = "Member1" };
        var member2 = new AppUser { Id = 3, UserName = "Member2" };

        var household = new Household(1, "Test House", "INV123", "Description")
        {
            Users = new List<AppUser> { member1, member2 }
        };

        var user = new AppUser { Id = 1, Household = household };

        _userRepositoryMock.Setup(x => x.GetUserWithHouseholdByIdAsync(1)).ReturnsAsync(user);

        _userManagerMock.Setup(x => x.GetRolesAsync(member1)).ReturnsAsync(new List<string> { "Adult" });
        _userManagerMock.Setup(x => x.GetRolesAsync(member2)).ReturnsAsync(new List<string> { "Child" });

        // Act
        var result = await _service.GetUsersHouseholdMembersWithRolesAsync(1);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(x => x.UserName == "Member1" && x.RoleName == "Adult");
        result.Should().Contain(x => x.UserName == "Member2" && x.RoleName == "Child");
    }

    [Fact]
    public async Task GetUsersHouseholdAdultsAsync_ShouldReturnOnlyAdults()
    {
        // Arrange
        var adultUser = new AppUser { Id = 2 };
        var childUser = new AppUser { Id = 3 };

        var household = new Household(1, "Test House", "INV123", "Description")
        {
            Users = new List<AppUser> { adultUser, childUser }
        };

        var currentUser = new AppUser { Id = 1, Household = household };

        _userRepositoryMock.Setup(x => x.GetUserWithHouseholdByIdAsync(1)).ReturnsAsync(currentUser);

        _userManagerMock.Setup(x => x.GetUsersInRoleAsync(AuthConstants.RoleAdult))
            .ReturnsAsync(new List<AppUser> { adultUser });

        // Act
        var result = await _service.GetUsersHouseholdAdultsAsync(1);

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(2);
    }

    [Fact]
    public async Task GetUsersHouseholdChildrensAsync_ShouldReturnOnlyChildren()
    {
        // Arrange
        var adultUser = new AppUser { Id = 2 };
        var childUser = new AppUser { Id = 3 };

        var household = new Household(1, "Test House", "INV123", "Description")
        {
            Users = new List<AppUser> { adultUser, childUser }
        };

        var currentUser = new AppUser { Id = 1, Household = household };

        _userRepositoryMock.Setup(x => x.GetUserWithHouseholdByIdAsync(1)).ReturnsAsync(currentUser);

        _userManagerMock.Setup(x => x.GetUsersInRoleAsync(AuthConstants.RoleChild))
            .ReturnsAsync(new List<AppUser> { childUser });

        // Act
        var result = await _service.GetUsersHouseholdChildrensAsync(1);

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(3);
    }

    // --- Role Management Tests ---

    [Fact]
    public async Task UpdateUserRoleAsync_ShouldThrow_WhenRoleDoesNotExist()
    {
        // Arrange
        var user = new AppUser { Id = 1 };
        _userManagerMock.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
        _roleManagerMock.Setup(x => x.RoleExistsAsync("NonExistant")).ReturnsAsync(false);

        // Act
        var act = async () => await _service.UpdateUserRoleAsync(1, "NonExistant");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Role 'NonExistant' does not exist.");
    }

    [Fact]
    public async Task UpdateUserRoleAsync_ShouldSwitchRole_WhenDifferent()
    {
        // Arrange
        var user = new AppUser { Id = 1 };
        var newRole = "Adult";
        var oldRole = "Child";

        _userManagerMock.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
        _roleManagerMock.Setup(x => x.RoleExistsAsync(newRole)).ReturnsAsync(true);
        _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { oldRole });

        _userManagerMock.Setup(x => x.RemoveFromRoleAsync(user, oldRole))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.AddToRoleAsync(user, newRole))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _service.UpdateUserRoleAsync(1, newRole);

        // Assert
        result.Should().BeTrue();
        _userManagerMock.Verify(x => x.RemoveFromRoleAsync(user, oldRole), Times.Once);
        _userManagerMock.Verify(x => x.AddToRoleAsync(user, newRole), Times.Once);
    }

    [Fact]
    public async Task GetAvailableRolesAsync_ShouldReturnAllRoles()
    {
        // Arrange
        var roles = new List<IdentityRole<int>>
        {
            new IdentityRole<int>("Adult"),
            new IdentityRole<int>("Child")
        }.AsQueryable();

        _roleManagerMock.Setup(r => r.Roles).Returns(roles);

        // Act
        var result = await _service.GetAvailableRolesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain("Adult");
        result.Should().Contain("Child");
    }
}
