using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Shared.Chat;
using System.Security.Claims;

namespace ChoreBuddies.Backend.Features.Chat;

[Authorize]
public class ChatHub(ChoreBuddiesDbContext context, TimeProvider timeProvider) : Hub
{
    public async Task JoinHouseholdChat(int householdId)
    {
        // TODO dodać weryfikację, czy user faktycznie należy do tego domu!
        string groupName = GetGroupName(householdId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task SendMessage(int householdId, string messageContent)
    {
        var userIdStr = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
        {
            throw new HubException("Unauthorized");
        }

        // TODO brać z ClaimTypes.Name, jeśli tam jest
        var user = await context.Users.FindAsync(userId);
        if (user == null) return;

        // Save to database
        var newMessage = new ChatMessage(userId, householdId, messageContent, timeProvider.GetUtcNow());

        context.ChatMessages.Add(newMessage);
        await context.SaveChangesAsync();

        // Return DTO to group
        var messageDto = new ChatMessageDto
        (
            newMessage.Id,
            user.UserName ?? "Unknown",
            newMessage.Content,
            newMessage.SentAt,
            false // receiver will see as not mine
        );

        // TODO wyślij do wszystkich (Clients.Group), wtedy nadawca dostanie potwierdzenie "z serwera"
        string groupName = GetGroupName(householdId);
        await Clients.Group(groupName).SendAsync("ReceiveMessage", messageDto);
    }

    public async Task SendTyping(int householdId)
    {
        // TODO poprawić
        var userName = Context.User?.Identity?.Name ?? "Ktoś";
        string groupName = GetGroupName(householdId);

        await Clients.GroupExcept(groupName, Context.ConnectionId)
                     .SendAsync("UserIsTyping", userName);
    }

    // Pomocnicza metoda do tworzenia nazw grup
    private static string GetGroupName(int householdId) => $"Household_{householdId}";
}
