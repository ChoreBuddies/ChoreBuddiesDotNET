using AutoMapper;
using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Notifications;
using System.Security.Claims;

namespace ChoreBuddies.Backend.Features.Notifications.NotificationPreferences;

[ApiController]
[Route("/api/v1/notifications/preferences")]
[Authorize]
public class NotificationPreferenceController(INotificationPreferenceService notificationPreferenceService, IAppUserService userService, IMapper mapper) : ControllerBase
{
    private readonly INotificationPreferenceService _notificationPreferenceService = notificationPreferenceService;
    private readonly IMapper _mapper = mapper;
    private readonly IAppUserService _userService = userService;
    private async Task<AppUser?> GetCurrentUser()
    {
        var emailJWT = User.FindFirst(ClaimTypes.Email)?.Value;
        if (emailJWT == null)
            return null;
        return await _userService.GetUserByEmailAsync(emailJWT);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyPreferences()
    {
        var user = await GetCurrentUser();
        if (user == null)
            return BadRequest();
        var result = await _notificationPreferenceService.GetAllUserConfigAsync(user);
        return Ok(_mapper.Map<IEnumerable<NotificationPreferenceDto>>(result));
    }
    [HttpPut]
    public async Task<IActionResult> UpdateMyPreference([FromBody] NotificationPreferenceDto updatedPreference)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await GetCurrentUser();
        if (user == null)
            return BadRequest();
        try
        {
            await _notificationPreferenceService.UpdatePreferenceAsync(user, updatedPreference);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Failed to update preference: {ex.Message}");
        }
    }
}
