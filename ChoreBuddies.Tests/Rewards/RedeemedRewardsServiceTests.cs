using AutoMapper;
using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Notifications;
using ChoreBuddies.Backend.Features.RedeemedRewards;
using ChoreBuddies.Backend.Features.RedeemRewards;
using ChoreBuddies.Backend.Features.Rewards;
using ChoreBuddies.Backend.Features.Users;
using FluentAssertions;
using Moq;
using Shared.Rewards;

namespace ChoreBuddies.Tests.Rewards;

public class RedeemedRewardsServiceTests
{
    private readonly Mock<IRedeemedRewardsRepository> _redeemedRepo = new();
    private readonly Mock<IRewardsRepository> _rewardRepo = new();
    private readonly Mock<IAppUserRepository> _userRepo = new();
    private readonly Mock<IAppUserService> _userService = new();
    private readonly Mock<INotificationService> _notificationService = new();
    private readonly Mock<IMapper> _mapper = new();

    private readonly RedeemedRewardsService _service;

    public RedeemedRewardsServiceTests()
    {
        _service = new RedeemedRewardsService(
            _redeemedRepo.Object,
            _rewardRepo.Object,
            _userRepo.Object,
            _userService.Object,
            _notificationService.Object,
            _mapper.Object
        );
    }

    // -------------------------------------------------------
    // GetHouseholdsRedeemedRewardsAsync
    // -------------------------------------------------------

    [Fact]
    public async Task GetHouseholdsRedeemedRewardsAsync_ShouldReturnMappedList()
    {
        var redeemed = new List<RedeemedReward> { new RedeemedReward { Id = 1, Name = "Test", Description = "test" } };
        var redeemedDto = new List<RedeemedRewardDto> { new RedeemedRewardDto(1, 99, "Test", "test", 10, true) };

        _redeemedRepo.Setup(r => r.GetUsersRedeemedRewardsAsync(99)).ReturnsAsync(redeemed);
        _mapper.Setup(m => m.Map<List<RedeemedRewardDto>>(redeemed)).Returns(redeemedDto);

        var result = await _service.GetHouseholdsRedeemedRewardsAsync(99);

        result.Should().NotBeNull();
        result.Should().ContainSingle();
        result.First().Id.Should().Be(1);
    }

    // -------------------------------------------------------
    // GetUsersRedeemedRewardsAsync
    // -------------------------------------------------------

    [Fact]
    public async Task GetUsersRedeemedRewardsAsync_ShouldReturnMappedList()
    {
        var redeemed = new List<RedeemedReward> { new RedeemedReward { Id = 10, Name = "Test", Description = "test" } };
        var redeemedDto = new List<RedeemedRewardDto> { new RedeemedRewardDto(10, 99, "Test", "test", 10, true) };

        _redeemedRepo.Setup(r => r.GetUsersRedeemedRewardsAsync(5)).ReturnsAsync(redeemed);
        _mapper.Setup(m => m.Map<List<RedeemedRewardDto>>(redeemed)).Returns(redeemedDto);

        var result = await _service.GetUsersRedeemedRewardsAsync(5);

        result.Should().NotBeNull();
        result.Should().ContainSingle();
        result.First().Id.Should().Be(10);
    }

    // -------------------------------------------------------
    // RedeemRewardAsync
    // -------------------------------------------------------

    [Fact]
    public async Task RedeemRewardAsync_ShouldReturnNull_WhenUserNotFound()
    {
        _rewardRepo.Setup(r => r.GetRewardByIdAsync(3)).ReturnsAsync(new Reward("Test", "test", 1, 10, 1));
        _userRepo.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync((AppUser?)null);

        var result = await _service.RedeemRewardAsync(1, 3, false);

        result.Should().BeNull();
    }

    [Fact]
    public async Task RedeemRewardAsync_ShouldReturnNull_WhenRewardNotFound()
    {
        _rewardRepo.Setup(r => r.GetRewardByIdAsync(3)).ReturnsAsync((Reward?)null);
        _userRepo.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync(new AppUser());

        var result = await _service.RedeemRewardAsync(1, 3, false);

        result.Should().BeNull();
    }

    [Fact]
    public async Task RedeemRewardAsync_ShouldThrow_WhenRewardUnavailable()
    {
        var reward = new Reward("Test", "test", 1, 10, 0);
        var user = new AppUser { PointsCount = 100 };

        _rewardRepo.Setup(r => r.GetRewardByIdAsync(5)).ReturnsAsync(reward);
        _userRepo.Setup(r => r.GetUserByIdAsync(2)).ReturnsAsync(user);

        Func<Task> act = async () => await _service.RedeemRewardAsync(2, 5, false);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("This reward cannot be redeemed");
    }

    [Fact]
    public async Task RedeemRewardAsync_ShouldThrow_WhenUserDoesNotHaveEnoughPoints()
    {
        var reward = new Reward("Test", "test", 1, 50, 5);
        var user = new AppUser { PointsCount = 20 };

        _rewardRepo.Setup(r => r.GetRewardByIdAsync(3)).ReturnsAsync(reward);
        _userRepo.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync(user);

        Func<Task> act = async () => await _service.RedeemRewardAsync(1, 3, false);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User does not have enough points");
    }

    [Fact]
    public async Task RedeemRewardAsync_ShouldDeductPoints_AndReduceQuantity_AndReturnDto()
    {
        var reward = new Reward("Test", "test", 55, 40, 3)
        {
            Id = 10
        };

        var user = new AppUser
        {
            Id = 1,
            PointsCount = 100
        };

        var redeemedEntity = new RedeemedReward { Id = 88, Name = "Test", Description = "test" };
        var redeemedDto = new RedeemedRewardDto(88, 1, "Test", "test", 10, true) { Id = 88 };

        _rewardRepo.Setup(r => r.GetRewardByIdAsync(10)).ReturnsAsync(reward);
        _userRepo.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync(user);
        _redeemedRepo.Setup(r => r.RedeemRewardAsync(It.IsAny<RedeemedReward>()))
            .ReturnsAsync(redeemedEntity);
        _mapper.Setup(m => m.Map<RedeemedRewardDto>(redeemedEntity)).Returns(redeemedDto);

        var result = await _service.RedeemRewardAsync(1, 10, true);

        // Assertions
        result.Should().NotBeNull();
        result.Id.Should().Be(88);

        reward.QuantityAvailable.Should().Be(2);
        user.PointsCount.Should().Be(60); // 100 - 40
    }
}

