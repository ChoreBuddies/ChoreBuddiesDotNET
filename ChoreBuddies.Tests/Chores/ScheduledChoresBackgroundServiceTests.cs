using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Households;
using ChoreBuddies.Backend.Features.Households.Exceptions;
using ChoreBuddies.Backend.Features.ScheduledChores;
using ChoreBuddies.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Shared.Chores;
using Shared.PredefinedChores;

namespace ChoreBuddies.Tests.Chores;

public class ScheduledChoresBackgroundServiceTests : IDisposable
{
    private readonly FakeTimeProvider _fakeTimeProvider;
    private readonly Mock<IHouseholdService> _householdServiceMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<IServiceScope> _scopeMock;
    private readonly ChoreBuddiesDbContext _dbContext;

    public ScheduledChoresBackgroundServiceTests()
    {
        _fakeTimeProvider = new FakeTimeProvider();
        _fakeTimeProvider.SetUtcNow(new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero));

        var options = new DbContextOptionsBuilder<ChoreBuddiesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unikalna nazwa dla każdego testu
            .Options;
        _dbContext = new ChoreBuddiesDbContext(options, _fakeTimeProvider);

        _householdServiceMock = new Mock<IHouseholdService>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _scopeMock = new Mock<IServiceScope>();

        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(_scopeFactoryMock.Object);

        _scopeFactoryMock
            .Setup(x => x.CreateScope())
            .Returns(_scopeMock.Object);

        var scopedServiceProviderMock = new Mock<IServiceProvider>();
        _scopeMock.Setup(x => x.ServiceProvider).Returns(scopedServiceProviderMock.Object);

        scopedServiceProviderMock
            .Setup(x => x.GetService(typeof(ChoreBuddiesDbContext)))
            .Returns(_dbContext);

        scopedServiceProviderMock
            .Setup(x => x.GetService(typeof(IHouseholdService)))
            .Returns(_householdServiceMock.Object);
    }

    private async Task RunServiceOnceAsync()
    {
        var service = new ScheduledChoresBackgroundService(_serviceProviderMock.Object, _fakeTimeProvider);
        using var cts = new CancellationTokenSource();

        var executeTask = service.StartAsync(cts.Token);

        _fakeTimeProvider.Advance(TimeSpan.FromDays(1).Add(TimeSpan.FromMinutes(1)));

        await Task.Delay(200);

        cts.Cancel();

        try
        {
            await executeTask;
        }
        catch (OperationCanceledException) { }
    }
    [Fact]
    public async Task Should_CreateChore_When_ScheduledChore_IsDue()
    {
        // Arrange
        var scheduledChore = new ScheduledChore(
            name: "Daily Task", description: "Desc", userId: 1, room: "Kitchen",
            everyX: 1, frequency: Frequency.Daily, rewardPointsCount: 10, householdId: 100, choreDuration: 15)
        {
            Id = 1,
            LastGenerated = _fakeTimeProvider.GetUtcNow().Date.AddDays(-2)
        };

        _dbContext.ScheduledChores.Add(scheduledChore);
        await _dbContext.SaveChangesAsync();

        // Act
        await RunServiceOnceAsync();

        // Assert
        var createdChore = await _dbContext.Chores.FirstOrDefaultAsync();
        var updatedScheduled = await _dbContext.ScheduledChores.FirstAsync();

        Assert.NotNull(createdChore);
        Assert.Equal("Daily Task", createdChore.Name);
        Assert.Equal(Status.Assigned, createdChore.Status);

        Assert.Equal(_fakeTimeProvider.GetUtcNow().Date, updatedScheduled.LastGenerated);
    }

    [Fact]
    public async Task Should_Not_CreateChore_When_Not_Due()
    {
        // Arrange
        var scheduledChore = new ScheduledChore(
             name: "Weekly Task", description: "Desc", userId: 1, room: "Kitchen",
             everyX: 1, frequency: Frequency.Weekly, rewardPointsCount: 10, householdId: 100, choreDuration: 15)
        {
            Id = 1,
            LastGenerated = _fakeTimeProvider.GetUtcNow().Date
        };

        _dbContext.ScheduledChores.Add(scheduledChore);
        await _dbContext.SaveChangesAsync();

        // Act
        await RunServiceOnceAsync();

        // Assert
        var createdChore = await _dbContext.Chores.FirstOrDefaultAsync();
        Assert.Null(createdChore);
    }

    [Fact]
    public async Task Should_AutoAssignUser_When_UserId_IsNull()
    {
        // Arrange
        var scheduledChore = new ScheduledChore(
                name: "Auto Assign", description: "Desc", userId: null, room: "Kitchen",
                everyX: 1, frequency: Frequency.Daily, rewardPointsCount: 10, householdId: 100, choreDuration: 15)
        {
            Id = 1,
            LastGenerated = null
        };

        _dbContext.ScheduledChores.Add(scheduledChore);
        await _dbContext.SaveChangesAsync();

        _householdServiceMock
            .Setup(s => s.GetUserIdForAutoAssignAsync(scheduledChore.Id))
            .ReturnsAsync(99);

        // Act
        await RunServiceOnceAsync();

        // Assert
        var createdChore = await _dbContext.Chores.FirstAsync();
        Assert.Equal(99, createdChore.UserId);
        Assert.Equal(Status.Assigned, createdChore.Status);
    }

    [Fact]
    public async Task Should_Delete_ScheduledChore_When_HouseholdDoesNotExist()
    {
        // Arrange
        var scheduledChore = new ScheduledChore(
             name: "Orphan Task", description: "Desc", userId: null, room: "Kitchen",
             everyX: 1, frequency: Frequency.Daily, rewardPointsCount: 10, householdId: 999, choreDuration: 15)
        {
            Id = 1,
            LastGenerated = null
        };

        _dbContext.ScheduledChores.Add(scheduledChore);
        await _dbContext.SaveChangesAsync();

        // Mockujemy rzucenie wyjątku
        _householdServiceMock
            .Setup(s => s.GetUserIdForAutoAssignAsync(scheduledChore.Id))
            .ThrowsAsync(new HouseholdDoesNotExistException(scheduledChore.HouseholdId));

        // Act
        await RunServiceOnceAsync();
        // Assert
        var choreExists = await _dbContext.ScheduledChores.AnyAsync();
        var createdChore = await _dbContext.Chores.AnyAsync();

        Assert.False(choreExists, "ScheduledChore should be deleted");
        Assert.False(createdChore, "No actual Chore should be created");
    }

    [Fact]
    public async Task Should_CreateUnassignedChore_When_HouseholdHasNoUsers()
    {
        // Arrange
        var scheduledChore = new ScheduledChore(
             name: "Lonely Task", description: "Desc", userId: null, room: "Kitchen",
             everyX: 1, frequency: Frequency.Daily, rewardPointsCount: 10, householdId: 100, choreDuration: 15)
        {
            Id = 1,
            LastGenerated = null
        };

        _dbContext.ScheduledChores.Add(scheduledChore);
        await _dbContext.SaveChangesAsync();

        _householdServiceMock
            .Setup(s => s.GetUserIdForAutoAssignAsync(scheduledChore.Id))
            .ThrowsAsync(new HouseholdHasNoUsersException(scheduledChore.HouseholdId));

        // Act
        await RunServiceOnceAsync();
        // Assert
        var createdChore = await _dbContext.Chores.FirstAsync();

        Assert.Null(createdChore.UserId);
        Assert.Equal(Status.Unassigned, createdChore.Status);

        Assert.True(await _dbContext.ScheduledChores.AnyAsync());
    }

    [Fact]
    public async Task Should_Calculate_NextDueDate_Correctly_ForWeekly()
    {
        // Arrange
        var scheduledChore = new ScheduledChore(
             name: "Weekly", description: "Desc", userId: 1, room: "Kitchen",
             everyX: 1, frequency: Frequency.Weekly, rewardPointsCount: 10, householdId: 100, choreDuration: 15)
        {
            Id = 1,
            LastGenerated = null
        };

        _dbContext.ScheduledChores.Add(scheduledChore);
        await _dbContext.SaveChangesAsync();

        // Act
        await RunServiceOnceAsync();

        // Assert
        var createdChore = await _dbContext.Chores.FirstAsync();

        var loopDate = _fakeTimeProvider.GetUtcNow().Date;
        var expectedDueDate = loopDate.AddDays(7);

        Assert.Equal(expectedDueDate, createdChore.DueDate);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
