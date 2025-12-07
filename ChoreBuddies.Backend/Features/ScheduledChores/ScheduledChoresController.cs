using ChoreBuddies.Backend.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.ScheduledChores;
using System.Security.Claims;

namespace ChoreBuddies.Backend.Features.ScheduledChores;

[ApiController]
[Route("api/v1/Scheduledchores")]
[Authorize]
public class ScheduledChoresController : ControllerBase
{
    private readonly IScheduledChoresService _ScheduledChoresService;
    private readonly ITokenService _tokenService;

    public ScheduledChoresController(IScheduledChoresService tasksService, ITokenService tokenService)
    {
        _ScheduledChoresService = tasksService;
        _tokenService = tokenService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ScheduledChoreDto>> GetChore(int id)
    {
        var result = await _ScheduledChoresService.GetChoreDetailsAsync(id);
        return Ok(result);
    }
    [HttpPost("update")]
    public async Task<ActionResult<ScheduledChoreDto>> UpdateChore([FromBody] ScheduledChoreDto ScheduledChoreDto)
    {
        var result = await _ScheduledChoresService.UpdateChoreAsync(ScheduledChoreDto);
        return Ok(result);
    }
    [HttpPost("add")]
    public async Task<ActionResult<ScheduledChoreDto>> AddChore([FromBody] CreateScheduledChoreDto createScheduledChoreDto)
    {
        var householdId = _tokenService.GetHouseholdIdFromToken(User);

        var result = await _ScheduledChoresService.CreateChoreAsync(createScheduledChoreDto, householdId);
        return Ok(result);
    }
    [HttpDelete("delete/{id}")]
    public async Task<ActionResult<ScheduledChoreDto>> DeleteChore(int id)
    {
        var result = await _ScheduledChoresService.DeleteChoreAsync(id);
        return Ok(result);
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ScheduledChoreDto>>> GetUsersChores([FromQuery] int? userId)
    {
        if (userId is not null)
        {
            var result = await _ScheduledChoresService.GetUsersChoreDetailsAsync((int)userId);
            return Ok(result);
        }
        else
        {
            var myUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (myUserId is null)
                return Unauthorized();

            var result = await _ScheduledChoresService.GetUsersChoreDetailsAsync(int.Parse(myUserId));
            return Ok(result);
        }
    }
    [HttpGet("HouseholdChores")]
    public async Task<ActionResult<IEnumerable<ScheduledChoreDto>>> GetMyHouseholdChores()
    {
        var userId = _tokenService.GetUserIdFromToken(User);

        var result = await _ScheduledChoresService.GetMyHouseholdChoreDetailsAsync(userId);
        return Ok(result);
    }
}
