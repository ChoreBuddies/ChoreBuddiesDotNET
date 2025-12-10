using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Users;
using ChoreBuddies.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ChoreBuddies.Tests.Database;

public class AppUserRepositoryTests : BaseIntegrationTest
{
    private readonly AppUserRepository _repository;

    public AppUserRepositoryTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        _repository = new AppUserRepository(DbContext);
    }

    // --- READ ---

    [Fact]
    public async Task GetUserByEmailAsync_ExistingEmail_ReturnsUser()
    {
        // Arrange
        var email = $"exist_{Guid.NewGuid()}@test.com";
        var user = new AppUser
        {
            UserName = $"User_{Guid.NewGuid()}",
            Email = email,
            FirstName = "John",
            LastName = "Doe"
        };

        DbContext.ApplicationUsers.Add(user);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be(email);
        result.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task GetUserByEmailAsync_NonExistingEmail_ReturnsNull()
    {
        // Act
        var result = await _repository.GetUserByEmailAsync("ghost@nonexistent.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserByIdAsync_ExistingId_ReturnsUser()
    {
        // Arrange
        var user = new AppUser
        {
            UserName = $"UserID_{Guid.NewGuid()}",
            Email = $"userid_{Guid.NewGuid()}@test.com"
        };

        DbContext.ApplicationUsers.Add(user);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.UserName.Should().Be(user.UserName);
    }

    [Fact]
    public async Task GetUserByIdAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetUserByIdAsync(999999);

        // Assert
        result.Should().BeNull();
    }

    // --- UPDATE & SAVE ---

    [Fact]
    public async Task UpdateUserAsync_ShouldUpdateUserProperties()
    {
        // Arrange
        var user = new AppUser
        {
            UserName = $"Update_{Guid.NewGuid()}",
            Email = $"update_{Guid.NewGuid()}@test.com",
            PointsCount = 0,
            FirstName = "OldName"
        };

        DbContext.ApplicationUsers.Add(user);
        await DbContext.SaveChangesAsync();

        // Act
        user.PointsCount = 100;
        user.FirstName = "NewName";
        user.RefreshToken = "NewRefreshToken123";

        await _repository.UpdateUserAsync(user);
        await _repository.SaveChangesAsync();

        // Assert
        var dbUser = await DbContext.ApplicationUsers.AsNoTracking().FirstOrDefaultAsync(u => u.Id == user.Id);

        dbUser.Should().NotBeNull();
        dbUser!.PointsCount.Should().Be(100);
        dbUser.FirstName.Should().Be("NewName");
        dbUser.RefreshToken.Should().Be("NewRefreshToken123");
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldPersistHouseholdAssignment()
    {
        // Arrange
        var owner = new AppUser { UserName = $"Owner_{Guid.NewGuid()}", Email = $"owner_{Guid.NewGuid()}@test.com" };
        DbContext.ApplicationUsers.Add(owner);
        await DbContext.SaveChangesAsync();

        var household = new Household(owner.Id, "Test House", "CODE12", "Desc");
        DbContext.Households.Add(household);
        await DbContext.SaveChangesAsync();

        var userToJoin = new AppUser { UserName = $"Joiner_{Guid.NewGuid()}", Email = $"joiner_{Guid.NewGuid()}@test.com" };
        DbContext.ApplicationUsers.Add(userToJoin);
        await DbContext.SaveChangesAsync();

        // Act
        userToJoin.HouseholdId = household.Id;

        await _repository.UpdateUserAsync(userToJoin);
        await _repository.SaveChangesAsync();

        // Assert
        var dbUser = await DbContext.ApplicationUsers.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userToJoin.Id);
        dbUser!.HouseholdId.Should().Be(household.Id);
    }
}
