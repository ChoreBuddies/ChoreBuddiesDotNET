using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Households;
using ChoreBuddies.Backend.Features.Users;
using ChoreBuddies.Backend.Infrastructure.Authentication;
using ChoreBuddies.Backend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Shared.Chat;
using System.Security.Claims;

namespace ChoreBuddies.Backend.Features.Chat;

[Authorize]
public class ChatHub(ChoreBuddiesDbContext context,
    ITokenService tokenService,
    IAppUserService userService,
    IHouseholdService householdService,
    TimeProvider timeProvider) : Hub
{
    public async Task JoinHouseholdChat(int householdId)
    {
        if (Context == null)
            throw new HubException("Unauthorized");

        // Check if user belongs to the household
        var userId = tokenService.GetUserIdFromToken(Context.User ?? new ClaimsPrincipal());
        bool hasAccess = await householdService.CheckIfUserBelongsAsync(householdId, userId);

        if (!hasAccess)
            throw new HubException("Unauthorized");

        string groupName = GetGroupName(householdId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task SendMessage(int householdId, string messageContent)
    {

        // Check if user belongs to the household
        var userId = tokenService.GetUserIdFromToken(Context?.User ?? new ClaimsPrincipal());
        bool hasAccess = await householdService.CheckIfUserBelongsAsync(householdId, userId);

        if (!hasAccess)
            throw new HubException("Unauthorized");

        var user = await userService.GetUserByIdAsync(userId);
        if (user == null)
            throw new HubException("User not found");

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

    private static string GetGroupName(int householdId) => $"Household_{householdId}";
}
