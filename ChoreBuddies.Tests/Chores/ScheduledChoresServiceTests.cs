namespace ChoreBuddies.Tests.Chores;
using AutoMapper;
using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.ScheduledChores;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.ScheduledChores;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class ScheduledChoresServiceTests
{
    private readonly Mock<IScheduledChoresRepository> _repoMock;
    private readonly IMapper _mapper;
    private readonly ScheduledChoresService _service;

    public ScheduledChoresServiceTests()
    {
        var loggerFactoryMock = new Mock<ILoggerFactory>();

        loggerFactoryMock
            .Setup(lf => lf.CreateLogger(It.IsAny<string>()))
            .Returns(new Mock<ILogger>().Object);
        _repoMock = new Mock<IScheduledChoresRepository>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ScheduledChore, ScheduledChoreDto>().ReverseMap();
            cfg.CreateMap<CreateScheduledChoreDto, ScheduledChore>();
        }, loggerFactoryMock.Object);

        _mapper = config.CreateMapper();

        _service = new ScheduledChoresService(_mapper, _repoMock.Object);
    }
    [Fact]
    public async Task GetChoreDetailsAsync_ReturnsChore_WhenFound()
    {
        // Arrange
        var chore = new ScheduledChore(
            name: "Test",
            description: "Test desc",
            userId: null,
            room: "Kitchen",
            everyX: 1,
            frequency: Frequency.Daily,
            rewardPointsCount: 10,
            householdId: 1
        )
        {
            Id = 1
        };
        _repoMock.Setup(r => r.GetChoreByIdAsync(1)).ReturnsAsync(chore);

        // Act
        var result = await _service.GetChoreDetailsAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
    }

    [Fact]
    public async Task GetChoreDetailsAsync_Throws_WhenNotFound()
    {
        // Arrange
        _repoMock.Setup(r => r.GetChoreByIdAsync(1)).ReturnsAsync((ScheduledChore?)null);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.GetChoreDetailsAsync(1));
    }

    [Fact]
    public async Task CreateChoreAsync_CreatesChore()
    {
        // Arrange
        var dto = new CreateScheduledChoreDto(Name: "Test", Description: "Some description", UserId: null, Room: "Kitchen", RewardPointsCount: 10, frequency: Frequency.Daily, minAge: 5, choreDuration: 1, everyX: 1);

        var createdChore = new ScheduledChore(name: "Test", description: "Some description", userId: null, room: "Kitchen", everyX: 1, frequency: Frequency.Daily, rewardPointsCount: 10, householdId: 5, minAge: 5, choreDuration: 1)
        {
            Id = 123
        };

        _repoMock
            .Setup(r => r.CreateChoreAsync(It.IsAny<ScheduledChore>()))
            .ReturnsAsync(createdChore);

        // Act
        var result = await _service.CreateChoreAsync(dto, householdId: 5);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(123, result.Id);
        Assert.Equal("Test", result.Name);
        Assert.Equal(5, result.HouseholdId);

        _repoMock.Verify(
            r => r.CreateChoreAsync(
                It.Is<ScheduledChore>(c => c.HouseholdId == 5)
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateChoreAsync_Updates_WhenChoreExists()
    {
        // Arrange
        var dto = new ScheduledChoreDto { Id = 1, Name = "Updated" };
        var existingChore = new ScheduledChore(name: "Old", description: "Some description", userId: null, room: "Kitchen", everyX: 1, frequency: Frequency.Daily, rewardPointsCount: 10, householdId: 5, minAge: 5, choreDuration: 1)
        {
            Id = 1
        };
        var updatedChore = new ScheduledChore(name: "Updated", description: "Some description", userId: null, room: "Kitchen", everyX: 1, frequency: Frequency.Daily, rewardPointsCount: 10, householdId: 5, minAge: 5, choreDuration: 1)
        {
            Id = 1
        };

        _repoMock.Setup(r => r.GetChoreByIdAsync(1)).ReturnsAsync(existingChore);
        _repoMock.Setup(r => r.UpdateChoreAsync(It.IsAny<ScheduledChore>()))
                 .ReturnsAsync(updatedChore);

        // Act
        var result = await _service.UpdateChoreAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated", result!.Name);
    }

    [Fact]
    public async Task UpdateChoreAsync_Throws_WhenChoreNotFound()
    {
        // Arrange
        var dto = new ScheduledChoreDto { Id = 1 };

        _repoMock.Setup(r => r.GetChoreByIdAsync(1))
                 .ReturnsAsync((ScheduledChore?)null);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.UpdateChoreAsync(dto));
    }

    [Fact]
    public async Task DeleteChoreAsync_Deletes_WhenChoreExists()
    {
        // Arrange
        var existingChore = new ScheduledChore(name: "Old", description: "Some description", userId: null, room: "Kitchen", everyX: 1, frequency: Frequency.Daily, rewardPointsCount: 10, householdId: 5, minAge: 5, choreDuration: 1)
        {
            Id = 1
        };
        var deletedChore = new ScheduledChore(name: "Old", description: "Some description", userId: null, room: "Kitchen", everyX: 1, frequency: Frequency.Daily, rewardPointsCount: 10, householdId: 5, minAge: 5, choreDuration: 1)
        {
            Id = 1
        };
        _repoMock.Setup(r => r.GetChoreByIdAsync(1)).ReturnsAsync(existingChore);
        _repoMock.Setup(r => r.DeleteChoreAsync(existingChore)).ReturnsAsync(deletedChore);

        // Act
        var result = await _service.DeleteChoreAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
    }

    [Fact]
    public async Task DeleteChoreAsync_Throws_WhenNotFound()
    {
        // Arramge
        _repoMock.Setup(r => r.GetChoreByIdAsync(1))
                 .ReturnsAsync((ScheduledChore?)null);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.DeleteChoreAsync(1));
    }

    [Fact]
    public async Task GetUsersChoreDetailsAsync_ReturnsMappedList()
    {
        // Arrange
        var list = new List<ScheduledChore> {
            new ScheduledChore(name: "A", description: "Some description", userId: null, room: "Kitchen", everyX: 1, frequency: Frequency.Daily, rewardPointsCount: 10, householdId: 5, minAge: 5, choreDuration: 1)
                {
                    Id = 1
                }
        };

        _repoMock.Setup(r => r.GetUsersChoresAsync(10)).ReturnsAsync(list);

        // Act
        var result = await _service.GetUsersChoreDetailsAsync(10);

        // Assert
        Assert.Single(result);
        Assert.Equal(1, ((List<ScheduledChoreDto>)result)[0].Id);
    }

    [Fact]
    public async Task GetMyHouseholdChoreDetailsAsync_ReturnsMappedList()
    {
        // Arrange
        var list = new List<ScheduledChore> {
            new ScheduledChore(name: "B", description: "Some description", userId: null, room: "Kitchen", everyX: 1, frequency: Frequency.Daily, rewardPointsCount: 10, householdId: 5, minAge: 5, choreDuration: 1)
                {
                    Id = 2
                }
        };

        _repoMock.Setup(r => r.GetHouseholdChoresAsync(10)).ReturnsAsync(list);

        // Act
        var result = await _service.GetMyHouseholdChoreDetailsAsync(10);

        // Assert
        Assert.Single(result);
        Assert.Equal(2, ((List<ScheduledChoreDto>)result)[0].Id);
    }
}
