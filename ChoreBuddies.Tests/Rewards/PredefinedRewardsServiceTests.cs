using AutoMapper;
using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.PredefinedRewards;
using FluentAssertions;
using Moq;
using Shared.PredefinedRewards;

namespace ChoreBuddies.Tests.Rewards;

public class PredefinedRewardsServiceTests
{
    private readonly Mock<IPredefinedRewardsRepository> _predefinedRewardRepo = new();
    private readonly Mock<IMapper> _mapper = new();

    private readonly PredefinedRewardsService _service;

    public PredefinedRewardsServiceTests()
    {
        _service = new PredefinedRewardsService(
            _predefinedRewardRepo.Object,
            _mapper.Object
        );
    }

    // -------------------------------------------------------
    // GetAllPredefinedRewardsAsync
    // -------------------------------------------------------

    [Fact]
    public async Task GetAllPredefinedRewardsAsync_ShouldReturnMappedList()
    {
        var predefinedRewards = new List<PredefinedReward>
        {
            new() { Id = 1, Name = "Reward 1", Description="Test" }
        };

        var predefinedRewardDtos = new List<PredefinedRewardDto>
        {
            new(1, "Reward 1", "test", 50, 4)
        };

        _predefinedRewardRepo
            .Setup(r => r.GetAllPredefinedRewardsAsync())
            .ReturnsAsync(predefinedRewards);

        _mapper
            .Setup(m => m.Map<List<PredefinedRewardDto>>(predefinedRewards))
            .Returns(predefinedRewardDtos);

        var result = await _service.GetAllPredefinedRewardsAsync();

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(1);
    }

    [Fact]
    public async Task GetAllPredefinedRewardsAsync_ShouldReturnEmptyList_WhenRepositoryReturnsEmpty()
    {
        var emptyList = new List<PredefinedReward>();
        var emptyDtoList = new List<PredefinedRewardDto>();

        _predefinedRewardRepo
            .Setup(r => r.GetAllPredefinedRewardsAsync())
            .ReturnsAsync(emptyList);

        _mapper
            .Setup(m => m.Map<List<PredefinedRewardDto>>(emptyList))
            .Returns(emptyDtoList);

        var result = await _service.GetAllPredefinedRewardsAsync();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    // -------------------------------------------------------
    // GetPredefinedRewardsAsync
    // -------------------------------------------------------

    [Fact]
    public async Task GetPredefinedRewardsAsync_ShouldReturnRewardsMatchingIds()
    {
        // Arrange
        var ids = new List<int> { 1, 3 };

        var predefinedRewards = new List<PredefinedReward>
        {
            new PredefinedReward { Id = 1, Name="name1", Description="dsc1", Cost=10, QuantityAvailable=100 },
            new PredefinedReward { Id = 3, Name="name3", Description="dsc3", Cost=30, QuantityAvailable=300  }
        };

        var predefinedRewardDtos = new List<PredefinedRewardDto>
        {
            new PredefinedRewardDto(1, "name1", "dsc1", 10, 100),
            new PredefinedRewardDto(3, "name3", "dsc3", 30, 300)
        };

        _predefinedRewardRepo
            .Setup(r => r.GetPredefinedRewardsAsync(ids))
            .ReturnsAsync(predefinedRewards);

        _mapper
            .Setup(m => m.Map<List<PredefinedRewardDto>>(predefinedRewards))
            .Returns(predefinedRewardDtos);

        // Act
        var result = await _service.GetPredefinedRewardsAsync(ids);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(predefinedRewardDtos);

        _predefinedRewardRepo.Verify(
            r => r.GetPredefinedRewardsAsync(ids),
            Times.Once);
    }
}

