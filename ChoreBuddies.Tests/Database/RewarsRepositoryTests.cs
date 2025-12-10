using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Rewards;
using ChoreBuddies.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ChoreBuddies.Tests.Database;

public class RewardsRepositoryTests : BaseIntegrationTest
{
    private readonly RewardsRepository _repository;

    public RewardsRepositoryTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        _repository = new RewardsRepository(DbContext);
    }

    // --- HELPERS ---
    private async Task<Household> CreateTestHouseholdAsync()
    {
        var owner = new AppUser { UserName = $"Owner_{Guid.NewGuid()}", Email = $"owner_{Guid.NewGuid()}@test.com" };
        DbContext.ApplicationUsers.Add(owner);
        await DbContext.SaveChangesAsync();

        var household = new Household(owner.Id, "Reward House", "REW_CD", "Desc");
        DbContext.Households.Add(household);
        await DbContext.SaveChangesAsync();

        return household;
    }

    // --- CREATE ---

    [Fact]
    public async Task CreateRewardAsync_ShouldAddRewardToDatabase()
    {
        // Arrange
        var household = await CreateTestHouseholdAsync();
        var reward = new Reward("Pizza Night", "Delicious pizza", household.Id, 500, 1);

        // Act
        var result = await _repository.CreateRewardAsync(reward);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().BeGreaterThan(0);

        var dbReward = await DbContext.Rewards.FindAsync(result.Id);
        dbReward.Should().NotBeNull();
        dbReward!.Name.Should().Be("Pizza Night");
        dbReward.Cost.Should().Be(500);
        dbReward.HouseholdId.Should().Be(household.Id);
    }

    // --- READ ---

    [Fact]
    public async Task GetRewardByIdAsync_ExistingId_ReturnsReward()
    {
        // Arrange
        var household = await CreateTestHouseholdAsync();
        var reward = new Reward("Cinema Ticket", "Movie", household.Id, 200, 5);
        DbContext.Rewards.Add(reward);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetRewardByIdAsync(reward.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Cinema Ticket");
    }

    [Fact]
    public async Task GetHouseholdRewardsAsync_ReturnsOnlyRewardsForSpecificHousehold()
    {
        // Arrange
        var household1 = await CreateTestHouseholdAsync();
        var household2 = await CreateTestHouseholdAsync();

        var reward1 = new Reward("H1 Reward", "Desc", household1.Id, 100, 1);
        var reward2 = new Reward("H2 Reward", "Desc", household2.Id, 100, 1);

        DbContext.Rewards.AddRange(reward1, reward2);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetHouseholdRewardsAsync(household1.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result!.First().Name.Should().Be("H1 Reward");
        result.Should().NotContain(r => r.Name == "H2 Reward");
    }

    // --- UPDATE ---

    [Fact]
    public async Task UpdateRewardAsync_ShouldUpdateValidFields()
    {
        // Arrange
        var household = await CreateTestHouseholdAsync();
        var reward = new Reward("Old Name", "Old Desc", household.Id, 100, 10);
        DbContext.Rewards.Add(reward);
        await DbContext.SaveChangesAsync();

        // Act
        var updateDto = new Reward("New Name", "New Desc", household.Id, 200, 5)
        {
            Id = reward.Id
        };

        var result = await _repository.UpdateRewardAsync(updateDto);

        // Assert
        result.Should().NotBeNull();

        var dbReward = await DbContext.Rewards.AsNoTracking().FirstOrDefaultAsync(r => r.Id == reward.Id);
        dbReward!.Name.Should().Be("New Name");
        dbReward.Description.Should().Be("New Desc");
        dbReward.Cost.Should().Be(200);
        dbReward.QuantityAvailable.Should().Be(5);
    }

    [Fact]
    public async Task UpdateRewardAsync_WithEmptyValues_ShouldNotOverwriteWithEmpty()
    {
        // Arrange
        var household = await CreateTestHouseholdAsync();
        var reward = new Reward("Original Name", "Original Desc", household.Id, 100, 10);
        DbContext.Rewards.Add(reward);
        await DbContext.SaveChangesAsync();

        // Act
        var updateDto = new Reward("", "", household.Id, -1, -1)
        {
            Id = reward.Id
        };

        var result = await _repository.UpdateRewardAsync(updateDto);

        // Assert
        var dbReward = await DbContext.Rewards.AsNoTracking().FirstOrDefaultAsync(r => r.Id == reward.Id);

        dbReward!.Name.Should().Be("Original Name");
        dbReward.Description.Should().Be("Original Desc");

        dbReward.Cost.Should().Be(100);
    }

    // --- DELETE ---

    [Fact]
    public async Task DeleteRewardAsync_ShouldRemoveFromDatabase()
    {
        // Arrange
        var household = await CreateTestHouseholdAsync();
        var reward = new Reward("To Delete", "Bye", household.Id, 10, 1);
        DbContext.Rewards.Add(reward);
        await DbContext.SaveChangesAsync();

        // Act
        await _repository.DeleteRewardAsync(reward);

        // Assert
        var dbReward = await DbContext.Rewards.FindAsync(reward.Id);
        dbReward.Should().BeNull();
    }
}
