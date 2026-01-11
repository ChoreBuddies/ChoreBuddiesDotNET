using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shared.Households;

namespace ChoreBuddies.Tests.Database;

public class HouseholdRepositoryTests : BaseIntegrationTest
{
    private readonly HouseholdRepository _repository;

    public HouseholdRepositoryTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        _repository = new HouseholdRepository(DbContext);
    }

    // --- CREATE ---

    [Fact]
    public async Task CreateHouseholdAsync_ShouldAddHouseholdToDatabase()
    {
        // Arrange
        var owner = new AppUser { UserName = "Owner_Create", Email = "owner_create@test.com" };
        DbContext.ApplicationUsers.Add(owner);
        await DbContext.SaveChangesAsync();

        var household = new Household(owner.Id, "My House", "INVCRT", "Desc");

        // Act
        var result = await _repository.CreateHouseholdAsync(household);

        // Assert
        result.Should().NotBeNull();
        var dbHousehold = await DbContext.Households.FirstOrDefaultAsync(h => h.InvitationCode == "INVCRT");
        dbHousehold.Should().NotBeNull();
        dbHousehold!.Name.Should().Be("My House");
        dbHousehold.OwnerId.Should().Be(owner.Id);
    }

    // --- READ ---

    [Fact]
    public async Task GetHouseholdByIdAsync_ExistingId_ReturnsHousehold()
    {
        // Arrange
        var owner = new AppUser { UserName = "Owner_GetId", Email = "owner_getid@test.com" };
        DbContext.ApplicationUsers.Add(owner);
        await DbContext.SaveChangesAsync();

        var household = new Household(owner.Id, "Find Me House", "INVFID", "Desc");
        DbContext.Households.Add(household);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetHouseholdByIdAsync(household.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(household.Id);
        result.Name.Should().Be("Find Me House");
    }

    [Fact]
    public async Task GetHouseholdByIdAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetHouseholdByIdAsync(999999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetHouseholdByInvitationCodeAsync_WithValidCode_ReturnsHousehold()
    {
        // Arrange
        var owner = new AppUser { UserName = "Owner_InvCode", Email = "owner_invcode@test.com" };
        DbContext.ApplicationUsers.Add(owner);
        await DbContext.SaveChangesAsync();

        var household = new Household(owner.Id, "Invite House", "CODU99", null);
        DbContext.Households.Add(household);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetHouseholdByInvitationCodeAsync("CODU99");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(household.Id);
    }

    // --- UPDATE ---

    [Fact]
    public async Task UpdateHouseholdAsync_ShouldUpdateProperties()
    {
        // Arrange
        var owner = new AppUser { UserName = "Owner_Update", Email = "owner_update@test.com" };
        DbContext.ApplicationUsers.Add(owner);
        await DbContext.SaveChangesAsync();

        var household = new Household(owner.Id, "Old Name", "INVUPD", "Old Desc");
        DbContext.Households.Add(household);
        await DbContext.SaveChangesAsync();

        // Act
        var householdToUpdate = await DbContext.Households.FindAsync(household.Id);
        var result = await _repository.UpdateHouseholdAsync(householdToUpdate!, "New Name", "New Desc");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("New Name");
        result.Description.Should().Be("New Desc");

        var dbHousehold = await DbContext.Households.AsNoTracking().FirstOrDefaultAsync(h => h.Id == household.Id);
        dbHousehold!.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task UpdateHouseholdAsync_WithNullValues_ShouldNotUpdateProperties()
    {
        // Arrange
        var owner = new AppUser { UserName = "Owner_UpdateNull", Email = "owner_updatenull@test.com" };
        DbContext.ApplicationUsers.Add(owner);
        await DbContext.SaveChangesAsync();

        var household = new Household(owner.Id, "Original Name", "INVUPN", "Original Desc");
        DbContext.Households.Add(household);
        await DbContext.SaveChangesAsync();

        // Act
        var householdToUpdate = await DbContext.Households.FindAsync(household.Id);
        var result = await _repository.UpdateHouseholdAsync(householdToUpdate!, null, "");

        // Assert
        result!.Name.Should().Be("Original Name");
        result.Description.Should().Be("Original Desc");
    }

    // --- JOIN ---

    [Fact]
    public async Task JoinHouseholdAsync_ShouldAddUserToHousehold()
    {
        // Arrange
        var owner = new AppUser { UserName = "Owner_Join", Email = "owner_join@test.com" };
        var joiner = new AppUser { UserName = "Joiner", Email = "joiner@test.com" };
        DbContext.ApplicationUsers.AddRange(owner, joiner);
        await DbContext.SaveChangesAsync();

        var household = new Household(owner.Id, "Join House", "INVJIN", "Desc");
        DbContext.Households.Add(household);
        await DbContext.SaveChangesAsync();

        // Act
        var householdTracked = await DbContext.Households
            .Include(h => h.Users)
            .FirstAsync(h => h.Id == household.Id);

        var result = await _repository.JoinHouseholdAsync(householdTracked, joiner);

        // Assert
        result.Should().NotBeNull();
        result!.Users.Should().Contain(u => u.Id == joiner.Id);

        var checkDb = await DbContext.Households
            .Include(h => h.Users)
            .FirstAsync(h => h.Id == household.Id);
        checkDb.Users.Should().Contain(u => u.Id == joiner.Id);
    }

    // --- DELETE ---

    [Fact]
    public async Task DeleteHouseholdAsync_ShouldRemoveFromDatabase()
    {
        // Arrange
        var owner = new AppUser { UserName = "Owner_Delete", Email = "owner_delete@test.com" };
        DbContext.ApplicationUsers.Add(owner);
        await DbContext.SaveChangesAsync();

        var household = new Household(owner.Id, "Delete House", "INVDEL", "Desc");
        DbContext.Households.Add(household);
        await DbContext.SaveChangesAsync();

        // Act
        await _repository.DeleteHouseholdAsync(household);

        // Assert
        var deletedHousehold = await DbContext.Households.FindAsync(household.Id);
        deletedHousehold.Should().BeNull();
    }

    // --- CHECK PERMISSIONS ---

    [Fact]
    public async Task CheckIfUserBelongsAsync_UserInHousehold_ReturnsTrue()
    {
        // Arrange
        var user = new AppUser { UserName = "Member", Email = "member@test.com" };
        DbContext.ApplicationUsers.Add(user);
        await DbContext.SaveChangesAsync();

        var household = new Household(user.Id, "Member House", "INVMEM", "Desc");
        // Dodajemy usera bezpośrednio do listy użytkowników
        household.Users.Add(user);

        DbContext.Households.Add(household);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.CheckIfUserBelongsAsync(household.Id, user.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckIfUserBelongsAsync_UserNotInHousehold_ReturnsFalse()
    {
        // Arrange
        var owner = new AppUser { UserName = "Owner_Stranger", Email = "owner_stranger@test.com" };
        var stranger = new AppUser { UserName = "Stranger", Email = "stranger@test.com" };
        DbContext.ApplicationUsers.AddRange(owner, stranger);
        await DbContext.SaveChangesAsync();

        var household = new Household(owner.Id, "Private House", "INVPRI", "Desc");
        // Stranger NIE jest dodany do users
        DbContext.Households.Add(household);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.CheckIfUserBelongsAsync(household.Id, stranger.Id);

        // Assert
        result.Should().BeFalse();
    }
}
