using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Households;
using ChoreBuddies.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ChoreBuddies.Tests.Database;

public class HouseholdRepositoryTests : BaseIntegrationTest
{
    private HouseholdRepository _repository = null!;

    public HouseholdRepositoryTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        _repository = new HouseholdRepository(DbContext);
    }

    [Fact]
    public async Task CreateHouseholdAsync_ShouldAddHouseholdToDatabase()
    {
        // Arrange
        var owner = new AppUser { UserName = "Owner", Email = "owner@test.com" };
        DbContext.ApplicationUsers.Add(owner);
        await DbContext.SaveChangesAsync();

        var household = new Household(owner.Id, "My House", "INV123", "Desc");

        // Act
        var result = await _repository.CreateHouseholdAsync(household);

        // Assert
        result.Should().NotBeNull();
        var dbHousehold = await DbContext.Households.FirstOrDefaultAsync(h => h.InvitationCode == "INV123");
        dbHousehold.Should().NotBeNull();
        dbHousehold!.Name.Should().Be("My House");
        dbHousehold.OwnerId.Should().Be(owner.Id);
    }

    [Fact]
    public async Task GetHouseholdByInvitationCodeAsync_WithValidCode_ReturnsHousehold()
    {
        // Arrange
        var owner = new AppUser { Id = 0, UserName = "Owner2", Email = "owner2@test.com" };
        DbContext.ApplicationUsers.Add(owner);
        await DbContext.SaveChangesAsync();

        var household = new Household(0, "Test House", "CODE99", null);

        DbContext.Households.Add(household);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetHouseholdByInvitationCodeAsync("CODE99");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(household.Id);
    }
}
