using AutoMapper;
using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.DefaultRewards;
using FluentAssertions;
using Moq;
using Shared.Rewards;
using Xunit;

namespace ChoreBuddies.Tests.Rewards;

public class DefaultRewardsServiceTests
{
    private readonly Mock<IDefaultRewardsRepository> _defaultRewardRepo = new();
    private readonly Mock<IMapper> _mapper = new();

    private readonly DefaultRewardsService _service;

    public DefaultRewardsServiceTests()
    {
        _service = new DefaultRewardsService(
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

        var defaultRewardDtos = new List<DefaultRewardDto>
        {
            new(1, "Reward 1", "test", 50)
        };

        _defaultRewardRepo
            .Setup(r => r.GetAllDefaultRewardsAsync())
            .ReturnsAsync(defaultRewards);

        _mapper
            .Setup(m => m.Map<List<DefaultRewardDto>>(defaultRewards))
            .Returns(defaultRewardDtos);

        var result = await _service.GetAllDefaultRewardsAsync();

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(1);
    }

    [Fact]
    public async Task GetAllDefaultRewardsAsync_ShouldReturnEmptyList_WhenRepositoryReturnsEmpty()
    {
        var emptyList = new List<PredefinedReward>();
        var emptyDtoList = new List<DefaultRewardDto>();

        _defaultRewardRepo
            .Setup(r => r.GetAllDefaultRewardsAsync())
            .ReturnsAsync(emptyList);

        _mapper
            .Setup(m => m.Map<List<DefaultRewardDto>>(emptyList))
            .Returns(emptyDtoList);

        var result = await _service.GetAllDefaultRewardsAsync();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}

