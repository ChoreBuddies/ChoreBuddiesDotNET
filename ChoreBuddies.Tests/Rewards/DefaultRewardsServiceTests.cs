using AutoMapper;
using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.PredefinedRewards;
using FluentAssertions;
using Moq;
using Shared.PredefinedRewards;
using Xunit;

namespace ChoreBuddies.Tests.Rewards;

public class DefaultRewardsServiceTests
{
    private readonly Mock<IPredefinedRewardsRepository> _defaultRewardRepo = new();
    private readonly Mock<IMapper> _mapper = new();

    private readonly PredefinedRewardsService _service;

    public DefaultRewardsServiceTests()
    {
        _service = new PredefinedRewardsService(
            _defaultRewardRepo.Object,
            _mapper.Object
        );
    }

    // -------------------------------------------------------
    // GetAllDefaultRewardsAsync
    // -------------------------------------------------------

    [Fact]
    public async Task GetAllDefaultRewardsAsync_ShouldReturnMappedList()
    {
        var defaultRewards = new List<PredefinedReward>
        {
            new() { Id = 1, Name = "Reward 1", Description="Test" }
        };

        var defaultRewardDtos = new List<PredefinedRewardDto>
        {
            new(1, "Reward 1", "test", 50, 4)
        };

        _defaultRewardRepo
            .Setup(r => r.GetAllPredefinedRewardsAsync())
            .ReturnsAsync(defaultRewards);

        _mapper
            .Setup(m => m.Map<List<PredefinedRewardDto>>(defaultRewards))
            .Returns(defaultRewardDtos);

        var result = await _service.GetAllPredefinedRewardsAsync();

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(1);
    }

    [Fact]
    public async Task GetAllDefaultRewardsAsync_ShouldReturnEmptyList_WhenRepositoryReturnsEmpty()
    {
        var emptyList = new List<PredefinedReward>();
        var emptyDtoList = new List<PredefinedRewardDto>();

        _defaultRewardRepo
            .Setup(r => r.GetAllPredefinedRewardsAsync())
            .ReturnsAsync(emptyList);

        _mapper
            .Setup(m => m.Map<List<PredefinedRewardDto>>(emptyList))
            .Returns(emptyDtoList);

        var result = await _service.GetAllPredefinedRewardsAsync();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}

