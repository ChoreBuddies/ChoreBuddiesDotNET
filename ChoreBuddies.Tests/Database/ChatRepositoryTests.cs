using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Chat;
using ChoreBuddies.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ChoreBuddies.Tests.Database;

public class ChatRepositoryTests : BaseIntegrationTest
{
    private readonly ChatRepository _repository;

    public ChatRepositoryTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        _repository = new ChatRepository(DbContext);
    }

    // --- CREATE ---

    [Fact]
    public async Task CreateChatMessageAsync_ValidMessage_PersistsAndReturnsMessage()
    {
        // Arrange
        var user = await CreateUserAsync();
        var household = await CreateHouseholdAsync(user.Id);

        var newMessage = new ChatMessage(
            user.Id,
            household.Id,
            "Hello Integration Test!",
            DateTime.UtcNow
        );

        // Act
        var result = await _repository.CreateChatMessageAsync(newMessage);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().BeGreaterThan(0);
        result.Content.Should().Be("Hello Integration Test!");
        result.SenderId.Should().Be(user.Id);

        var dbMessage = await DbContext.ChatMessages.FirstOrDefaultAsync(m => m.Id == result.Id);
        dbMessage.Should().NotBeNull();
        dbMessage!.Content.Should().Be("Hello Integration Test!");
    }

    // --- READ / GET ---

    [Fact]
    public async Task GetNewestMessagesAsync_ShouldReturnMessagesForSpecificHouseholdOnly()
    {
        // Arrange
        var user = await CreateUserAsync();
        var householdTarget = await CreateHouseholdAsync(user.Id);
        var householdOther = await CreateHouseholdAsync(user.Id);

        var msgTarget = new ChatMessage(
            user.Id,
            householdTarget.Id,
            "Target Message",
            DateTime.UtcNow
        );

        var msgOther = new ChatMessage(
            user.Id,
            householdOther.Id,
            "Other Message",
            DateTime.UtcNow
        );

        DbContext.ChatMessages.AddRange(msgTarget, msgOther);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetNewestMessagesAsync(householdTarget.Id, 10);

        // Assert
        result.Should().HaveCount(1);
        result.First().Content.Should().Be("Target Message");
        result.First().HouseholdId.Should().Be(householdTarget.Id);
    }

    [Fact]
    public async Task GetNewestMessagesAsync_ShouldReturnMessagesOrderedBySentAtDescending()
    {
        // Arrange
        var user = await CreateUserAsync();
        var household = await CreateHouseholdAsync(user.Id);

        var msgOld = new ChatMessage
        (
            user.Id,
            household.Id,
            "Old",
            DateTime.UtcNow.AddHours(-2)
        );
        var msgNew = new ChatMessage
        (
            user.Id,
            household.Id,
            "New",
            DateTime.UtcNow
        );
        var msgMiddle = new ChatMessage
        (
            user.Id,
            household.Id,
            "Middle",
            DateTime.UtcNow.AddHours(-1)
        );

        DbContext.ChatMessages.AddRange(msgOld, msgNew, msgMiddle);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetNewestMessagesAsync(household.Id, 10);

        // Assert
        result.Should().HaveCount(3);

        result[0].Content.Should().Be("New");
        result[1].Content.Should().Be("Middle");
        result[2].Content.Should().Be("Old");
    }

    [Fact]
    public async Task GetNewestMessagesAsync_ShouldRespectNumberOfMessagesLimit()
    {
        // Arrange
        var user = await CreateUserAsync();
        var household = await CreateHouseholdAsync(user.Id);

        var messages = new List<ChatMessage>();
        for (int i = 0; i < 10; i++)
        {
            messages.Add(new ChatMessage
            (
                user.Id,
                household.Id,
                $"Msg {i}",
                DateTime.UtcNow.AddMinutes(i)
            ));
        }

        DbContext.ChatMessages.AddRange(messages);
        await DbContext.SaveChangesAsync();

        // Act
        var limit = 3;
        var result = await _repository.GetNewestMessagesAsync(household.Id, limit);

        // Assert
        result.Should().HaveCount(limit);
        result.First().Content.Should().Be("Msg 9");
        result.Last().Content.Should().Be("Msg 7");
    }

    [Fact]
    public async Task GetNewestMessagesAsync_ShouldIncludeSenderData()
    {
        // Arrange
        var user = await CreateUserAsync();
        var household = await CreateHouseholdAsync(user.Id);

        var msg = new ChatMessage
        (
            user.Id,
            household.Id,
            "Test Include",
            DateTime.UtcNow
        );

        DbContext.ChatMessages.Add(msg);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetNewestMessagesAsync(household.Id, 10);

        // Assert
        var message = result.First();
        message.Sender.Should().NotBeNull();
        message.Sender!.UserName.Should().Be(user.UserName);
    }

    // --- Helpers ---

    private async Task<AppUser> CreateUserAsync()
    {
        var user = new AppUser
        {
            UserName = $"User_{Guid.NewGuid()}",
            Email = $"user_{Guid.NewGuid()}@test.com",
            FirstName = "Test",
            LastName = "User"
        };
        DbContext.ApplicationUsers.Add(user);
        await DbContext.SaveChangesAsync();
        return user;
    }

    private async Task<Household> CreateHouseholdAsync(int ownerId)
    {
        var household = new Household(ownerId, $"House_{Guid.NewGuid()}", $"COD{Guid.NewGuid().ToString().Substring(0, 3)}", "Desc");
        DbContext.Households.Add(household);
        await DbContext.SaveChangesAsync();
        return household;
    }
}
