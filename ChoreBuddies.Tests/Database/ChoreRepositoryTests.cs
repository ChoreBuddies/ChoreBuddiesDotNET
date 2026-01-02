using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Chores;
using ChoreBuddies.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shared.Chores;

namespace ChoreBuddies.Tests.Database;

public class ChoresRepositoryTests : BaseIntegrationTest
{
    private readonly ChoresRepository _repository;

    public ChoresRepositoryTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        _repository = new ChoresRepository(DbContext);
    }

    // --- HELPERS ---
    private async Task<Household> CreateTestHouseholdAsync()
    {
        var owner = new AppUser { UserName = $"Owner_{Guid.NewGuid()}", Email = $"owner_{Guid.NewGuid()}@test.com" };
        DbContext.ApplicationUsers.Add(owner);
        await DbContext.SaveChangesAsync();

        var household = new Household(owner.Id, "Test House", "INVCOD", "Desc");
        DbContext.Households.Add(household);
        await DbContext.SaveChangesAsync();

        return household;
    }

    // --- CREATE ---

    [Fact]
    public async Task CreateChoreAsync_ShouldAddChoreToDatabase()
    {
        // Arrange
        var household = await CreateTestHouseholdAsync();
        var chore = new Chore("Wash Dishes", "Kitchen", null, household.Id, DateTime.UtcNow.AddDays(1), Status.Unassigned, "Kitchen", 10);

        // Act
        var result = await _repository.CreateChoreAsync(chore);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().BeGreaterThan(0);

        var dbChore = await DbContext.Chores.FindAsync(result.Id);
        dbChore.Should().NotBeNull();
        dbChore!.Name.Should().Be("Wash Dishes");
        dbChore.Status.Should().Be(Status.Unassigned);
    }

    [Fact]
    public async Task CreateChoreListAsync_ShouldAddMultipleChores()
    {
        // Arrange
        var household = await CreateTestHouseholdAsync();
        var chores = new List<Chore>
        {
            new Chore("Task 1", "D1", null, household.Id, null, Status.Unassigned, "Living Room", 5),
            new Chore("Task 2", "D2", null, household.Id, null, Status.Unassigned, "Bedroom", 5)
        };

        // Act
        var result = await _repository.CreateChoreListAsync(chores);

        // Assert
        result.Should().HaveCount(2);
        var count = await DbContext.Chores.CountAsync(c => c.HouseholdId == household.Id);
        count.Should().Be(2);
    }

    // --- READ ---

    [Fact]
    public async Task GetChoreByIdAsync_ExistingId_ReturnsChore()
    {
        // Arrange
        var household = await CreateTestHouseholdAsync();
        var chore = new Chore("Find Me", "Hidden", null, household.Id, null, Status.Unassigned, "Hall", 20);

        DbContext.Chores.Add(chore);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetChoreByIdAsync(chore.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Find Me");
    }

    [Fact]
    public async Task GetUsersChoresAsync_UserHasAssignedChores_ReturnsOnlyUserChores()
    {
        // Arrange
        var household = await CreateTestHouseholdAsync();

        var user = new AppUser { UserName = "ChoreUser", Email = "choreuser@test.com" };
        DbContext.ApplicationUsers.Add(user);
        await DbContext.SaveChangesAsync();

        var myChore = new Chore("My Task", "Desc", user.Id, household.Id, null, Status.Assigned, "Room", 10);
        var otherChore = new Chore("Other Task", "Desc", null, household.Id, null, Status.Unassigned, "Room", 10);

        DbContext.Chores.AddRange(myChore, otherChore);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetUsersChoresAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain(c => c.Name == "My Task");
        result.Should().NotContain(c => c.Name == "Other Task");
    }

    [Fact]
    public async Task GetHouseholdChoresAsync_UserInHousehold_ReturnsAllHouseholdChores()
    {
        // Arrange
        var owner = new AppUser { UserName = "HHMember", Email = "hhmember@test.com" };
        DbContext.ApplicationUsers.Add(owner);
        await DbContext.SaveChangesAsync();

        var household = new Household(owner.Id, "Chore House", "CHORE1", "Desc");
        household.Users.Add(owner);

        DbContext.Households.Add(household);
        await DbContext.SaveChangesAsync();

        var chore1 = new Chore("Vacuum", "Desc", null, household.Id, null, Status.Unassigned, "All", 50);
        var chore2 = new Chore("Dust", "Desc", owner.Id, household.Id, null, Status.Assigned, "All", 20);

        DbContext.Chores.AddRange(chore1, chore2);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetHouseholdChoresAsync(owner.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result!.Select(c => c.Name).Should().Contain(new[] { "Vacuum", "Dust" });
    }

    // --- UPDATE ---

    [Fact]
    public async Task UpdateChoreAsync_ShouldModifyChoreProperties()
    {
        // Arrange
        var household = await CreateTestHouseholdAsync();
        var chore = new Chore("Old Name", "Old Desc", null, household.Id, null, Status.Unassigned, "Old Room", 5);
        DbContext.Chores.Add(chore);
        await DbContext.SaveChangesAsync();

        // Act
        chore.Name = "New Name";
        chore.Status = Status.Completed;

        var result = await _repository.UpdateChoreAsync(chore);

        // Assert
        result.Should().NotBeNull();

        var dbChore = await DbContext.Chores.AsNoTracking().FirstOrDefaultAsync(c => c.Id == chore.Id);
        dbChore!.Name.Should().Be("New Name");
        dbChore.Status.Should().Be(Status.Completed);
    }

    [Fact]
    public async Task AssignChoreAsync_ShouldSetUserIdAndStatus()
    {
        // Arrange
        var household = await CreateTestHouseholdAsync();
        var user = new AppUser { UserName = "Assignee", Email = "assignee@test.com" };
        DbContext.ApplicationUsers.Add(user);
        await DbContext.SaveChangesAsync();

        var chore = new Chore("To Assign", "Desc", null, household.Id, null, Status.Unassigned, "Room", 10);
        DbContext.Chores.Add(chore);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.AssignChoreAsync(chore.Id, user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(user.Id);
    }

    // --- DELETE ---

    [Fact]
    public async Task DeleteChoreAsync_ShouldRemoveFromDatabase()
    {
        // Arrange
        var household = await CreateTestHouseholdAsync();
        var chore = new Chore("To Delete", "Bye", null, household.Id, null, Status.Unassigned, "Void", 1);
        DbContext.Chores.Add(chore);
        await DbContext.SaveChangesAsync();

        // Act
        await _repository.DeleteChoreAsync(chore);

        // Assert
        var dbChore = await DbContext.Chores.FindAsync(chore.Id);
        dbChore.Should().BeNull();
    }
}
