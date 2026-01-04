using ChoreBuddies.Backend.Features.Households;
using ChoreBuddies.Backend.Features.Notifications;
using ChoreBuddies.Backend.Features.Users;
using ChoreBuddies.Backend.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Shared.Chat;
using System.Security.Claims;

namespace ChoreBuddies.Backend.Features.Chat;

[Authorize]
public class ChatHub(IChatService chatService,
    ITokenService tokenService,
    IAppUserService userService,
    IHouseholdService householdService,
    IServiceScopeFactory scopeFactory) : Hub
{
    private readonly IChatService _chatService = chatService;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IAppUserService _userService = userService;
    private readonly IHouseholdService _householdService = householdService;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    public async Task JoinHouseholdChat(int householdId)
    {
        if (Context == null)
            throw new HubException("Unauthorized");

        // Check if user belongs to the household
        var userId = _tokenService.GetUserIdFromToken(Context.User ?? new ClaimsPrincipal());
        bool hasAccess = await _householdService.CheckIfUserBelongsAsync(householdId, userId);

        if (!hasAccess)
            throw new HubException("Unauthorized");

        string groupName = _chatService.GetGroupName(householdId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task SendMessage(int householdId, string messageContent, Guid clientUniqueId)
    {

        // Check if user belongs to the household
        var userId = _tokenService.GetUserIdFromToken(Context?.User ?? new ClaimsPrincipal());
        bool hasAccess = await _householdService.CheckIfUserBelongsAsync(householdId, userId);

        if (!hasAccess)
            throw new HubException("Unauthorized");

        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
            throw new HubException("User not found");

        // Return DTO to group
        var messageDto = await _chatService.CreateChatMessageAsync(user, messageContent, clientUniqueId);

        if (messageDto == null)
            throw new HubException("Failed to create message");

        string groupName = _chatService.GetGroupName(householdId);

        // 4. Send to receivers (IsMine = false)
        await Clients.OthersInGroup(groupName).SendAsync(ChatConstants.ReceiveMessage, messageDto);

        // 5. Send to Caller (IsMine = true)
        await Clients.Caller.SendAsync(ChatConstants.ReceiveMessage, messageDto with { IsMine = true });

        // 6. Send Notifications
        var allMembers = await _userService.GetUsersHouseholdMembersAsync(user.Id);
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
            using var scope = _scopeFactory.CreateScope();

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
                            recipient.Id,
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
        var userName = _tokenService.GetUserNameFromToken(Context?.User ?? new ClaimsPrincipal()) ?? "Somebody";
        string groupName = _chatService.GetGroupName(householdId);

        await Clients.GroupExcept(groupName, Context?.ConnectionId ?? string.Empty)
                     .SendAsync(ChatConstants.UserIsTyping, userName);
    }
}
