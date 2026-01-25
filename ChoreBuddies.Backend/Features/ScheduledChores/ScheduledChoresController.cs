using ChoreBuddies.Backend.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Authentication;
using Shared.PredefinedChores;
using System.Security.Claims;

namespace ChoreBuddies.Backend.Features.ScheduledChores;

[ApiController]
[Route("api/v1/scheduledChores")]
[Authorize]
public class ScheduledChoresController : ControllerBase
{
    private readonly IScheduledChoresService _scheduledChoresService;
    private readonly ITokenService _tokenService;

    public ScheduledChoresController(IScheduledChoresService tasksService, ITokenService tokenService)
    {
        _scheduledChoresService = tasksService;
        _tokenService = tokenService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ScheduledChoreDto>> GetChore(int id)
    {
        var result = await _scheduledChoresService.GetChoreDetailsAsync(id);
        return Ok(result);
    }

    [HttpPost("update")]
    [Authorize(Roles = AuthConstants.RoleAdult)]
    public async Task<ActionResult<ScheduledChoreDto>> UpdateChore([FromBody] ScheduledChoreDto ScheduledChoreDto)
    {
        var result = await _scheduledChoresService.UpdateChoreAsync(ScheduledChoreDto);
        return Ok(result);
    }

    [HttpPut("frequency")]
    [Authorize(Roles = AuthConstants.RoleAdult)]
    public async Task<ActionResult<ScheduledChoreTileViewDto>> UpdateChoreFrequency([FromBody] ScheduledChoreFrequencyUpdateDto scheduledChoreDto)
    {
        var result = await _scheduledChoresService.UpdateChoreFrequencyAsync(scheduledChoreDto.Id, scheduledChoreDto.Frequency);
        return Ok(result);
    }

    [HttpPost("add")]
    [Authorize(Roles = AuthConstants.RoleAdult)]
    public async Task<ActionResult<ScheduledChoreDto>> AddChore([FromBody] CreateScheduledChoreDto createScheduledChoreDto)
    {
        var householdId = _tokenService.GetHouseholdIdFromToken(User);

        var result = await _scheduledChoresService.CreateChoreAsync(createScheduledChoreDto, householdId);
        return Ok(result);
    }

    [HttpPost("add-predefined")]
    [Authorize(Roles = AuthConstants.RoleAdult)]
    public async Task<ActionResult<IEnumerable<ScheduledChoreDto>>> AddPredefinedChores([FromBody] PredefinedChoreRequest request)
    {
        var householdId = _tokenService.GetHouseholdIdFromToken(User);
        var result = await _scheduledChoresService.AddPredefinedChoresToHouseholdAsync(request.PredefinedChoreIds, householdId);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = AuthConstants.RoleAdult)]
    public async Task<ActionResult<ScheduledChoreDto>> DeleteChore(int id)
    {
        var result = await _scheduledChoresService.DeleteChoreAsync(id);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ScheduledChoreDto>>> GetUsersChores([FromQuery] int? userId)
    {
        if (userId is not null)
        {
            var result = await _scheduledChoresService.GetUsersChoreDetailsAsync((int)userId);
            return Ok(result);
        }
        else
        {
            var myUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (myUserId is null)
                return Unauthorized();

            var result = await _scheduledChoresService.GetUsersChoreDetailsAsync(int.Parse(myUserId));
            return Ok(result);
        }
    }

    [HttpGet("household-chores")]
    public async Task<ActionResult<IEnumerable<ScheduledChoreTileViewDto>>> GetMyHouseholdChores()
    {
        var userId = _tokenService.GetUserIdFromToken(User);

        var result = await _scheduledChoresService.GetMyHouseholdChoresDetailsAsync(userId);
        return Ok(result);
    }

    [HttpGet("Household-chores/overview")]
    public async Task<ActionResult<IEnumerable<ScheduledChoreTileViewDto>>> GetMyHouseholdChoresOverview()
    {
        var userId = _tokenService.GetUserIdFromToken(User);

        var result = await _scheduledChoresService.GetMyHouseholdChoresOverviewDetailsAsync(userId);
        return Ok(result);
    }
}
