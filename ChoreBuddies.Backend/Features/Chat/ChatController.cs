using ChoreBuddies.Backend.Features.Households;
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
public class ChatController(ChoreBuddiesDbContext context,
    ITokenService tokenService,
    IHouseholdService householdService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChatMessageDto>>> GetMessages()
    {
        var userId = tokenService.GetUserIdFromToken(User);
        var householdId = tokenService.GetHouseholdIdFromToken(User);

        // Check if user belongs to the household
        bool hasAccess = await householdService.CheckIfUserBelongsAsync(householdId, userId);

        if (!hasAccess)
            return Forbid();

        var messages = await context.ChatMessages
            .Where(m => m.HouseholdId == householdId)
            .OrderByDescending(m => m.SentAt)
            .Take(50)
            .Include(m => m.Sender)
            .Select(m => new ChatMessageDto(
                m.Id,
                (m.Sender != null && m.Sender.UserName != null) ? m.Sender.UserName : "Unknown",
                m.Content,
                m.SentAt,
                m.SenderId == userId,
                null
            ))
            .ToListAsync();

        // Reverse order to send from oldest to newest
        return Ok(messages.OrderBy(m => m.SentAt));
    }
}
