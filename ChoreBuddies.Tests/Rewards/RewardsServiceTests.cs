using AutoMapper;
using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.PredefinedRewards;
using ChoreBuddies.Backend.Features.Rewards;
using Moq;
using Shared.PredefinedRewards;
using Shared.Rewards;

namespace ChoreBuddies.Tests.Rewards;

public class RewardsServiceTests
{
    private readonly Mock<IRewardsRepository> _repository = new();
    private readonly Mock<IPredefinedRewardsService> _predefinedService = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly RewardsService _service;

    public RewardsServiceTests()
    {
        _service = new RewardsService(_mapper.Object, _repository.Object, _predefinedService.Object);
    }

    // -------------------------------------------------------
    // CreateRewardAsync
    // -------------------------------------------------------

    [Fact]
    public async Task CreateRewardAsync_ShouldCreateReward_AndReturnRewardDto()
    {
        var createDto = new CreateRewardDto("Test", "test", 1, 50, 1);
        var rewardEntity = new Reward("Test", "test", 1, 50, 1) { Id = 1 };
        var rewardDto = new RewardDto(1, "Test", "test", 1, 50, 1);

        _mapper.Setup(m => m.Map<Reward>(createDto)).Returns(rewardEntity);
        _repository.Setup(r => r.CreateRewardAsync(rewardEntity)).ReturnsAsync(rewardEntity);
        _mapper.Setup(m => m.Map<RewardDto>(rewardEntity)).Returns(rewardDto);

        var result = await _service.CreateRewardAsync(createDto);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    // -------------------------------------------------------
    // DeleteRewardAsync
    // -------------------------------------------------------

    [Fact]
    public async Task DeleteRewardAsync_ShouldDeleteReward_WhenExists()
    {
        var reward = new Reward("Test", "test", 1, 50, 1) { Id = 5 };
        var rewardDto = new RewardDto(5, "Test", "test", 1, 50, 1);

        _repository.Setup(r => r.GetRewardByIdAsync(5)).ReturnsAsync(reward);
        _repository.Setup(r => r.DeleteRewardAsync(reward)).ReturnsAsync(reward);
        _mapper.Setup(m => m.Map<RewardDto>(reward)).Returns(rewardDto);

        var result = await _service.DeleteRewardAsync(5);

        Assert.NotNull(result);
        Assert.Equal(5, result.Id);
    }

    [Fact]
    public async Task DeleteRewardAsync_ShouldThrow_WhenRewardNotFound()
    {
        _repository.Setup(r => r.GetRewardByIdAsync(5)).ReturnsAsync((Reward?)null);

        await Assert.ThrowsAsync<Exception>(() => _service.DeleteRewardAsync(5));
    }

    // -------------------------------------------------------
    // GetHouseholdRewardsAsync
    // -------------------------------------------------------

    [Fact]
    public async Task GetHouseholdRewardsAsync_ShouldReturnMappedList()
    {
        var rewards = new List<Reward> { new Reward("Test", "test", 1, 50, 1) { Id = 2 } };
        var rewardsDto = new List<RewardDto> { new RewardDto(2, "Test", "test", 1, 50, 1) };

        _repository.Setup(r => r.GetHouseholdRewardsAsync(10)).ReturnsAsync(rewards);
        _mapper.Setup(m => m.Map<List<RewardDto>>(rewards)).Returns(rewardsDto);

        var result = await _service.GetHouseholdRewardsAsync(10);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(2, result.First().Id);
    }

    // -------------------------------------------------------
    // GetRewardByIdAsync
    // -------------------------------------------------------

    [Fact]
    public async Task GetRewardByIdAsync_ShouldReturnRewardDto_WhenExists()
    {
        var reward = new Reward("Test", "test", 1, 50, 1) { Id = 7 };
        var rewardDto = new RewardDto(7, "Test", "test", 1, 50, 1);

        _repository.Setup(r => r.GetRewardByIdAsync(7)).ReturnsAsync(reward);
        _mapper.Setup(m => m.Map<RewardDto?>(reward)).Returns(rewardDto);

        var result = await _service.GetRewardByIdAsync(7);

        Assert.NotNull(result);
        Assert.Equal(7, result.Id);
    }

    [Fact]
    public async Task GetRewardByIdAsync_ShouldThrow_WhenNotFound()
    {
        _repository.Setup(r => r.GetRewardByIdAsync(7)).ReturnsAsync((Reward?)null);

        await Assert.ThrowsAsync<Exception>(() => _service.GetRewardByIdAsync(7));
    }

    // -------------------------------------------------------
    // UpdateRewardAsync
    // -------------------------------------------------------

    [Fact]
    public async Task UpdateRewardAsync_ShouldUpdateReward_WhenExists()
    {
        var rewardDto = new RewardDto(3, "Updated", "test", 1, 50, 1);
        var existingReward = new Reward("Updated", "test", 1, 50, 1) { Id = 3 };
        var updatedReward = new Reward("Updated", "test", 1, 50, 1) { Id = 3 };
        var updatedRewardDto = new RewardDto(3, "Updated", "test", 1, 50, 1);

        _repository.Setup(r => r.GetRewardByIdAsync(3)).ReturnsAsync(existingReward);
        _mapper.Setup(m => m.Map<Reward>(rewardDto)).Returns(updatedReward);
        _repository.Setup(r => r.UpdateRewardAsync(updatedReward)).ReturnsAsync(updatedReward);
        _mapper.Setup(m => m.Map<RewardDto>(updatedReward)).Returns(updatedRewardDto);

        var result = await _service.UpdateRewardAsync(rewardDto);

        Assert.NotNull(result);
        Assert.Equal("Updated", result.Name);
    }

    [Fact]
    public async Task UpdateRewardAsync_ShouldThrow_WhenRewardNotFound()
    {
        var rewardDto = new RewardDto(3, "Test", "test", 1, 50, 1); ;

        _repository.Setup(r => r.GetRewardByIdAsync(3)).ReturnsAsync((Reward?)null);

        await Assert.ThrowsAsync<Exception>(() => _service.UpdateRewardAsync(rewardDto));
    }

    // -------------------------------------------------------
    // AddPredefinedRewardsToHouseholdAsync
    // -------------------------------------------------------

    [Fact]
    public async Task AddPredefinedRewardsToHouseholdAsync_ShouldCreateRewards_AndReturnMappedDtos()
    {
        // Arrange
        var householdId = 10;
        var predefIds = new List<int> { 1, 2 };

        var predefRewards = new List<PredefinedRewardDto>
        {
            new PredefinedRewardDto(1, "Pizza Night", "Free pizza for everyone", 100, 1),
            new PredefinedRewardDto(2, "Extra Screen Time", "1 hour of gaming", 50, 5)
        };

        var createdReward1 = new Reward("Pizza Night", "Free pizza for everyone", 100, 1, householdId) { Id = 101 };
        var createdReward2 = new Reward("Extra Screen Time", "1 hour of gaming", 50, 5, householdId) { Id = 102 };

        var expectedDtos = new List<RewardDto>
        {
            new RewardDto(101, "Pizza Night", "Free pizza for everyone", 100, 1, householdId),
            new RewardDto(102, "Extra Screen Time", "1 hour of gaming", 50, 5, householdId)
        };

        _predefinedService.Setup(s => s.GetPredefinedRewardsAsync(predefIds)).ReturnsAsync(predefRewards);
        _repository.SetupSequence(r => r.CreateRewardAsync(It.IsAny<Reward>()))
            .ReturnsAsync(createdReward1)
            .ReturnsAsync(createdReward2);

        _mapper.Setup(m => m.Map<List<RewardDto>>(It.Is<List<Reward>>(l => l.Count == 2))).Returns(expectedDtos);

        // Act
        var result = await _service.AddPredefinedRewardsToHouseholdAsync(predefIds, householdId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Equal(101, result.First().Id);

        _repository.Verify(r => r.CreateRewardAsync(It.Is<Reward>(rew => rew.HouseholdId == householdId)), Times.Exactly(2));
    }

    [Fact]
    public async Task AddPredefinedRewardsToHouseholdAsync_ShouldThrowException_WhenRepositoryReturnsNull()
    {
        // Arrange
        var householdId = 10;
        var predefIds = new List<int> { 1 };
        var predefRewards = new List<PredefinedRewardDto>
        {
            new PredefinedRewardDto(1, "Error Reward", "Desc", 10, 1)
        };

        _predefinedService.Setup(s => s.GetPredefinedRewardsAsync(predefIds)).ReturnsAsync(predefRewards);

        _repository.Setup(r => r.CreateRewardAsync(It.IsAny<Reward>())).ReturnsAsync((Reward?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _service.AddPredefinedRewardsToHouseholdAsync(predefIds, householdId));

        Assert.Equal("Creating Reward from Predefined Reward Failed", ex.Message);
    }
}

