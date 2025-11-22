using ChoreBuddies.Backend.Features.Households;
using ChoreBuddies.Backend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Chat;
using System.Security.Claims;

namespace ChoreBuddies.Backend.Features.Chat;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize]
public class ChatController(ChoreBuddiesDbContext context, IHouseholdService householdService) : ControllerBase
{
    [HttpGet("{householdId}")]
    public async Task<ActionResult<IEnumerable<ChatMessageDto>>> GetMessages(int householdId)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out int currentUserId))
        {
            return Unauthorized();
        }

        // Check if user belongs to the household
        bool hasAccess = await householdService.CheckIfUserBelongsAsync(householdId, currentUserId);

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
                m.SenderId == currentUserId
            ))
            .ToListAsync();

        // Reverse order to send from oldest to newest
        return Ok(messages.OrderBy(m => m.SentAt));
    }
}
