using Shared.Households;
using ChoreBuddies.Backend.Infrastructure.Authentication;
using ChoreBuddies.Backend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Chat;

namespace ChoreBuddies.Backend.Features.Chat;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize]
public class ChatController(IChatService chatService,
    ITokenService tokenService,
    IHouseholdService householdService) : ControllerBase
{

    private readonly IChatService _chatService = chatService;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IHouseholdService _householdService = householdService;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChatMessageDto>>> GetMessages()
    {
        var userId = _tokenService.GetUserIdFromToken(User);
        var householdId = _tokenService.GetHouseholdIdFromToken(User);

        // Check if user belongs to the household
        bool hasAccess = await _householdService.CheckIfUserBelongsAsync(householdId, userId);

        if (!hasAccess)
            return Forbid();

        var messages = await _chatService.GetNewestMessagesAsync(userId, householdId);

        // Reverse order to send from oldest to newest
        return Ok(messages.OrderBy(m => m.SentAt));
    }
}
