using ChoreBuddies.Backend.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Reminders;

namespace ChoreBuddies.Backend.Features.Reminders;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class RemindersController(IRemindersService remindersService, ITokenService tokenService) : ControllerBase
{
    private readonly IRemindersService _remindersService = remindersService;
    private readonly ITokenService _tokenService = tokenService;

    [HttpPost()]
    public async Task<ActionResult> SetReminder([FromBody] ReminderDto reminderDto)
    {
        await _remindersService.SetReminder(_tokenService.GetUserIdFromToken(User), reminderDto);
        return Ok();
    }
}
