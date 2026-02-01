namespace ChoreBuddies.Tests.Chores;

using AutoMapper;
using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.PredefinedChores;
using ChoreBuddies.Backend.Features.ScheduledChores;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.PredefinedChores;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class ScheduledChoresServiceTests
{
    private readonly Mock<IScheduledChoresRepository> _repoMock;
    private readonly Mock<IPredefinedChoreService> _predefinedChoreServiceMock;
    private readonly IMapper _mapper;
    private readonly ScheduledChoresService _service;

    public ScheduledChoresServiceTests()
    {
        var loggerFactoryMock = new Mock<ILoggerFactory>();

        loggerFactoryMock
            .Setup(lf => lf.CreateLogger(It.IsAny<string>()))
            .Returns(new Mock<ILogger>().Object);
        _repoMock = new Mock<IScheduledChoresRepository>();
        _predefinedChoreServiceMock = new Mock<IPredefinedChoreService>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ScheduledChore, ScheduledChoreDto>().ReverseMap();
            cfg.CreateMap<CreateScheduledChoreDto, ScheduledChore>();
            cfg.CreateMap<ScheduledChore, ScheduledChoreTileViewDto>()
            .ForCtorParam(nameof(ScheduledChoreTileViewDto.UserName), opt => opt.MapFrom(src =>
                src.User != null ? src.User.UserName : null));
        }, loggerFactoryMock.Object);

        _mapper = config.CreateMapper();

        _service = new ScheduledChoresService(
            _mapper,
            _repoMock.Object,
            _predefinedChoreServiceMock.Object
        );
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
        var dto = new CreateScheduledChoreDto(Name: "Test", Description: "Some description", UserId: null, Room: "Kitchen", RewardPointsCount: 10, Frequency: Frequency.Daily, ChoreDuration: 1, EveryX: 1);

        var createdChore = new ScheduledChore(name: "Test", description: "Some description", userId: null, room: "Kitchen", everyX: 1, frequency: Frequency.Daily, rewardPointsCount: 10, householdId: 5, choreDuration: 1)
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
        var existingChore = new ScheduledChore(name: "Old", description: "Some description", userId: null, room: "Kitchen", everyX: 1, frequency: Frequency.Daily, rewardPointsCount: 10, householdId: 5, choreDuration: 1)
        {
            Id = 1
        };
        var updatedChore = new ScheduledChore(name: "Updated", description: "Some description", userId: null, room: "Kitchen", everyX: 1, frequency: Frequency.Daily, rewardPointsCount: 10, householdId: 5, choreDuration: 1)
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
        var existingChore = new ScheduledChore(name: "Old", description: "Some description", userId: null, room: "Kitchen", everyX: 1, frequency: Frequency.Daily, rewardPointsCount: 10, householdId: 5, choreDuration: 1)
        {
            Id = 1
        };
        var deletedChore = new ScheduledChore(name: "Old", description: "Some description", userId: null, room: "Kitchen", everyX: 1, frequency: Frequency.Daily, rewardPointsCount: 10, householdId: 5, choreDuration: 1)
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
            new ScheduledChore(name: "A", description: "Some description", userId: null, room: "Kitchen", everyX: 1, frequency: Frequency.Daily, rewardPointsCount: 10, householdId: 5, choreDuration: 1)
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
            new ScheduledChore(name: "B", description: "Some description", userId: null, room: "Kitchen", everyX: 1, frequency: Frequency.Daily, rewardPointsCount: 10, householdId: 5, choreDuration: 1)
                {
                    Id = 2
                }
        };

        _repoMock.Setup(r => r.GetHouseholdChoresAsync(10)).ReturnsAsync(list);

        // Act
        var result = await _service.GetMyHouseholdChoresDetailsAsync(10);

        // Assert
        Assert.Single(result);
        Assert.Equal(2, ((List<ScheduledChoreDto>)result)[0].Id);
    }
    //////////////////////////////

    [Fact]
    public async Task UpdateChoreFrequencyAsync_Updates_WhenSuccessful()
    {
        // Arrange
        int choreId = 1;
        var frequency = Frequency.Weekly;
        var resultChore = new ScheduledChore(name: "Test", description: "Desc", userId: null, room: "Living", everyX: 1, frequency: frequency, rewardPointsCount: 5, householdId: 1, choreDuration: 1)
        {
            Id = choreId
        };

        _repoMock.Setup(r => r.UpdateChoreFrequencyAsync(choreId, frequency))
                 .ReturnsAsync(resultChore);

        // Act
        var result = await _service.UpdateChoreFrequencyAsync(choreId, frequency);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(choreId, result.Id);
    }

    [Fact]
    public async Task UpdateChoreFrequencyAsync_Throws_WhenRepoReturnsNull()
    {
        // Arrange
        _repoMock.Setup(r => r.UpdateChoreFrequencyAsync(It.IsAny<int>(), It.IsAny<Frequency>()))
                 .ReturnsAsync((ScheduledChore?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _service.UpdateChoreFrequencyAsync(1, Frequency.Monthly));

        Assert.Equal("Updating Chore Frequency Failed", ex.Message);
    }

    [Fact]
    public async Task AddPredefinedChoresToHouseholdAsync_CreatesChores_WhenSuccessful()
    {
        // Arrange
        var predefinedIds = new List<int> { 10, 11 };
        int householdId = 99;

        var predefinedChores = new List<PredefinedChore>
        {
            new PredefinedChore {
                Id = 10,
                Name = "Clean Windows",
                Description = "Desc 1",
                Room = "Living Room",
                EveryX = 1,
                Frequency = Frequency.Monthly,
                RewardPointsCount = 30,
                ChoreDuration = 30
            },
            new PredefinedChore {
                Id = 11,
                Name = "Mow Lawn",
                Description = "Desc 2",
                Room = "Garden",
                EveryX = 1,
                Frequency = Frequency.Weekly,
                RewardPointsCount = 60,
                ChoreDuration = 60
            }
        };

        _predefinedChoreServiceMock
            .Setup(s => s.GetPredefinedChoresAsync(predefinedIds))
            .ReturnsAsync(predefinedChores);

        _repoMock
            .Setup(r => r.CreateChoreAsync(It.IsAny<ScheduledChore>()))
            .ReturnsAsync((ScheduledChore c) =>
            {
                c.Id = new Random().Next(1, 1000);
                return c;
            });

        // Act
        var result = await _service.AddPredefinedChoresToHouseholdAsync(predefinedIds, householdId);

        // Assert
        Assert.NotNull(result);
        var resultList = result as List<ScheduledChoreDto> ?? new List<ScheduledChoreDto>(result);
        Assert.Equal(2, resultList.Count);

        _repoMock.Verify(r => r.CreateChoreAsync(It.Is<ScheduledChore>(c => c.HouseholdId == householdId)), Times.Exactly(2));
    }

    [Fact]
    public async Task AddPredefinedChoresToHouseholdAsync_Throws_WhenCreateChoreFails()
    {
        // Arrange
        var predefinedIds = new List<int> { 10 };
        var predefinedChores = new List<PredefinedChore>
        {
            new PredefinedChore {
                Id = 10,
                Name = "Fail Chore",
                Description = "Desc",
                Room = "Room",
                EveryX = 1,
                Frequency = Frequency.Daily,
                RewardPointsCount = 10,
                ChoreDuration = 10
            }
        };

        _predefinedChoreServiceMock
            .Setup(s => s.GetPredefinedChoresAsync(predefinedIds))
            .ReturnsAsync(predefinedChores);

        _repoMock
            .Setup(r => r.CreateChoreAsync(It.IsAny<ScheduledChore>()))
            .ReturnsAsync((ScheduledChore?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _service.AddPredefinedChoresToHouseholdAsync(predefinedIds, 1));

        Assert.Equal("Creating Chore from Predefined Chore Failed", ex.Message);
    }

    [Fact]
    public async Task GetMyHouseholdChoresOverviewDetailsAsync_ReturnsMappedList()
    {
        // Arrange
        int userId = 5;
        var choresList = new List<ScheduledChore>
        {
            new ScheduledChore(name: "Task A", description: "D", userId: userId, room: "R", everyX: 1, frequency: Frequency.Daily, rewardPointsCount: 1, householdId: 1, choreDuration: 10) { Id = 1 },
            new ScheduledChore(name: "Task B", description: "D", userId: userId, room: "R", everyX: 1, frequency: Frequency.Daily, rewardPointsCount: 1, householdId: 1, choreDuration: 10) { Id = 2 }
        };

        _repoMock
            .Setup(r => r.GetHouseholdChoresWithUserAsync(userId))
            .ReturnsAsync(choresList);

        // Act
        var result = await _service.GetMyHouseholdChoresOverviewDetailsAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, ((List<ScheduledChoreTileViewDto>)result).Count);
    }
}
