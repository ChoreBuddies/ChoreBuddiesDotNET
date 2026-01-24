using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Households;
using ChoreBuddies.Backend.Features.Households.Exceptions;
using ChoreBuddies.Backend.Features.Users;
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
        _invitationService.Setup(x => x.GenerateUniqueInvitationCodeAsync())
            .ReturnsAsync("INV123");

        var created = new Household(1, "Test", "INV123", "");
        _householdRepo.Setup(x => x.CreateHouseholdAsync(It.IsAny<Household>()))
            .ReturnsAsync(created);

        _userService.Setup(x => x.GetUserByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new AppUser { Id = 10 });

        _householdRepo.Setup(x => x.GetHouseholdByInvitationCodeAsync("INV123"))
            .ReturnsAsync(created);

        var result = await _service.CreateHouseholdAsync(
            new CreateHouseholdDto("Test", ""),
            10);

        Assert.NotNull(result);
        Assert.Equal("INV123", result.InvitationCode);

        _invitationService.Verify(x => x.GenerateUniqueInvitationCodeAsync(), Times.Once);
        _householdRepo.Verify(x => x.CreateHouseholdAsync(It.IsAny<Household>()), Times.Once);
    }

    [Fact]
    public async Task CreateHouseholdAsync_ShouldThrow_WhenJoinFails()
    {
        _invitationService.Setup(x => x.GenerateUniqueInvitationCodeAsync())
            .ReturnsAsync("INV123");

        _householdRepo.Setup(x => x.CreateHouseholdAsync(It.IsAny<Household>()))
            .ReturnsAsync(new Household(1, "Test", "INV123", ""));

        _userService.Setup(x => x.GetUserByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((AppUser?)null);

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
        var household = new Household(99, "Test", "INV123", "");
        _householdRepo.Setup(x => x.GetHouseholdByIdAsync(household.Id))
            .ReturnsAsync(household);

        var result = await _service.GetUsersHouseholdAsync(household.Id);

        Assert.NotNull(result);
        Assert.Equal(household.Id, result.Id);
    }

    [Fact]
    public async Task GetUsersHouseholdAsync_ShouldReturnNull()
    {
        _householdRepo.Setup(x => x.GetHouseholdByIdAsync(1))
            .ReturnsAsync((Household?)null);

        var result = await _service.GetUsersHouseholdAsync(1);

        Assert.Null(result);
    }

    // -----------------------------
    // JoinHouseholdAsync tests
    // -----------------------------
    [Fact]
    public async Task JoinHouseholdAsync_ShouldJoinUser()
    {
        var user = new AppUser { Id = 10 };
        var household = new Household(77, "Test", "INV123", "");

        _userService.Setup(x => x.GetUserByIdAsync(10))
            .ReturnsAsync(user);

        _householdRepo.Setup(x => x.GetHouseholdByInvitationCodeAsync("INV123"))
            .ReturnsAsync(household);

        var result = await _service.JoinHouseholdAsync("INV123", 10);

        Assert.Equal(household, result);
        _householdRepo.Verify(x => x.JoinHouseholdAsync(household, user), Times.Once);
    }

    [Fact]
    public async Task JoinHouseholdAsync_ShouldThrow_WhenUserNotFound()
    {
        _userService.Setup(x => x.GetUserByIdAsync(10))
            .ReturnsAsync((AppUser?)null);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.JoinHouseholdAsync("INV123", 10));
    }

    [Fact]
    public async Task JoinHouseholdAsync_ShouldThrow_WhenHouseholdNotFound()
    {
        _userService.Setup(x => x.GetUserByIdAsync(10))
            .ReturnsAsync(new AppUser { Id = 10 });

        _householdRepo.Setup(x => x.GetHouseholdByInvitationCodeAsync("INV123"))
            .ReturnsAsync((Household?)null);

        await Assert.ThrowsAsync<InvalidInvitationCodeException>(() =>
            _service.JoinHouseholdAsync("INV123", 10));
    }

    // -----------------------------
    // UpdateHouseholdAsync tests
    // -----------------------------
    [Fact]
    public async Task UpdateHouseholdAsync_ShouldUpdate()
    {
        var household = new Household(3, "Test", "INV123", "");
        _householdRepo.Setup(x => x.GetHouseholdByIdAsync(household.Id))
            .ReturnsAsync(household);

        _householdRepo
            .Setup(x => x.UpdateHouseholdAsync(household, "NewName", "NewDesc"))
            .ReturnsAsync(new Household(3, "NewName", "INV123", description: "NewDesc"));

        var dto = new CreateHouseholdDto("NewName", "NewDesc");
        var result = await _service.UpdateHouseholdAsync(household.Id, dto);

        Assert.Equal("NewName", result!.Name);
        Assert.Equal("NewDesc", result.Description);
    }

    [Fact]
    public async Task UpdateHouseholdAsync_ShouldReturnNull_WhenNotFound()
    {
        _householdRepo.Setup(x => x.GetHouseholdByIdAsync(3))
            .ReturnsAsync((Household?)null);

        var result = await _service.UpdateHouseholdAsync(3, new CreateHouseholdDto("", null));

        Assert.Null(result);
    }

    // -----------------------------
    // DeleteHouseholdAsync tests
    // -----------------------------
    [Fact]
    public async Task DeleteHouseholdAsync_ShouldDelete()
    {
        var household = new Household(5, "Test", "INV123", "");
        _householdRepo.Setup(x => x.GetHouseholdByIdAsync(household.Id))
            .ReturnsAsync(household);

        _householdRepo.Setup(x => x.DeleteHouseholdAsync(household))
            .ReturnsAsync(household);

        var result = await _service.DeleteHouseholdAsync(household.Id);

        Assert.Equal(household.Id, result!.Id);
    }

    [Fact]
    public async Task DeleteHouseholdAsync_ShouldReturnNull_WhenNotFound()
    {
        _householdRepo.Setup(x => x.GetHouseholdByIdAsync(5))
            .ReturnsAsync((Household?)null);

        var result = await _service.DeleteHouseholdAsync(5);

        Assert.Null(result);
    }

    // -----------------------------
    // CheckIfUserBelongsAsync tests
    // -----------------------------
    [Fact]
    public async Task CheckIfUserBelongsAsync_ShouldReturnValue()
    {
        _householdRepo.Setup(x => x.CheckIfUserBelongsAsync(1, 10))
            .ReturnsAsync(true);

        var result = await _service.CheckIfUserBelongsAsync(1, 10);

        Assert.True(result);
    }
}
