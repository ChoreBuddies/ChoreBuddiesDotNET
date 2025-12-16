using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Households;
using ChoreBuddies.Backend.Features.Notifications;
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
IServiceScopeFactory scopeFactory,
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

    public async Task SendMessage(int householdId, string messageContent, Guid clientUniqueId)
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
            false, // default
            clientUniqueId
        );

        string groupName = GetGroupName(householdId);

        // 4. Send to receivers (IsMine = false)
        await Clients.OthersInGroup(groupName).SendAsync(ChatConstants.ReceiveMessage, messageDto);

        // 5. Send to Caller (IsMine = true)
        await Clients.Caller.SendAsync(ChatConstants.ReceiveMessage, messageDto with { IsMine = true });

        // 6. Send Notifications
        var allMembers = await userService.GetUsersHouseholdMembers(user.Id);
        var recipientIds = allMembers
            .Where(m => m.Id != user.Id)
            .Select(m => m.Id)
            .ToList();

        if (recipientIds.Any())
        {
            _ = SendPushNotificationsInBackground(recipientIds, user.UserName ?? "Domownik", messageContent);
        }
    }

    private async Task SendPushNotificationsInBackground(List<int> recipientIds, string senderName, string messageContent)
    {
        await Task.Run(async () =>
        {
            using var scope = scopeFactory.CreateScope();

            var scopedUserService = scope.ServiceProvider.GetRequiredService<IAppUserService>();
            var scopedNotificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            foreach (var recipientId in recipientIds)
            {
                try
                {
                    var recipient = await scopedUserService.GetUserByIdAsync(recipientId);

                    if (recipient != null)
                    {
                        await scopedNotificationService.SendNewMessageNotificationAsync(
                            recipient,
                            senderName,
                            messageContent
                        );
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Notification Sending Error for user {recipientId}: {ex.Message}");
                }
            }
        });
    }

    public async Task SendTyping(int householdId)
    {
        var userName = tokenService.GetUserNameFromToken(Context?.User ?? new ClaimsPrincipal()) ?? "Somebody";
        string groupName = GetGroupName(householdId);

        await Clients.GroupExcept(groupName, Context?.ConnectionId ?? string.Empty)
                     .SendAsync(ChatConstants.UserIsTyping, userName);
    }

    private static string GetGroupName(int householdId) => $"Household_{householdId}";
}
