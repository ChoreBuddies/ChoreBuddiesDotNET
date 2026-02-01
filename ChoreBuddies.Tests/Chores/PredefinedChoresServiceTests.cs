namespace ChoreBuddies.Tests.Chores;
using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.PredefinedChores;
using FluentAssertions;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class PredefinedChoreServiceTests
{
    private readonly Mock<IPredefinedChoreRepository> _repositoryMock;
    private readonly PredefinedChoreService _service;

    public PredefinedChoreServiceTests()
    {
        _repositoryMock = new Mock<IPredefinedChoreRepository>();
        _service = new PredefinedChoreService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetAllPredefinedChoresAsync_ShouldReturnAllPredefinedChores()
    {
        // Arrange
        var predefinedChores = new List<PredefinedChore>
        {
            new PredefinedChore { Id = 1 },
            new PredefinedChore { Id = 2 }
        };

        _repositoryMock
            .Setup(r => r.GetAllPredefinedChoreAsync())
            .ReturnsAsync(predefinedChores);

        // Act
        var result = await _service.GetAllPredefinedChoresAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(predefinedChores);

        _repositoryMock.Verify(
            r => r.GetAllPredefinedChoreAsync(),
            Times.Once);
    }

    [Fact]
    public async Task GetPredefinedChoresAsync_ShouldReturnChoresMatchingIds()
    {
        // Arrange
        var ids = new List<int> { 1, 3 };

        var predefinedChores = new List<PredefinedChore>
        {
            new PredefinedChore { Id = 1 },
            new PredefinedChore { Id = 3 }
        };

        _repositoryMock
            .Setup(r => r.GetPredefinedChoresAsync(ids))
            .ReturnsAsync(predefinedChores);

        // Act
        var result = await _service.GetPredefinedChoresAsync(ids);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(predefinedChores);

        _repositoryMock.Verify(
            r => r.GetPredefinedChoresAsync(ids),
            Times.Once);
    }
}
