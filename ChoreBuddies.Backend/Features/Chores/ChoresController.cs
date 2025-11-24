using Microsoft.AspNetCore.Mvc;
using Shared.Chores;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ChoreBuddies.Backend.Features.Chores;

[ApiController]
[Route("api/v1/chores")]
public class ChoresController : ControllerBase
{
    private readonly IChoresService _tasksService;

    public ChoresController(IChoresService tasksService)
    {
        _tasksService = tasksService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ChoreDto>> GetChore(string id)
    {
        var result = await _tasksService.GetChoreDetailsAsync(id);
        return Ok(result);
    }
    [HttpPost("update")]
    public async Task<ActionResult<ChoreDto>> UpdadeChore([FromBody] ChoreDto choreDto)
    {
        var result = await _tasksService.UpdateChoreAsync(choreDto);
        return Ok(result);
    }
    [HttpPost("add")]
    public async Task<ActionResult<ChoreDto>> AddChore([FromBody] CreateChoreDto createChoreDto)
    {
        var result = await _tasksService.CreateChoreAsync(createChoreDto);
        return Ok(result);
    }
    [HttpPost("addlist")]
    public async Task<ActionResult<IEnumerable<ChoreDto>>> AddChoreList([FromBody] IEnumerable<CreateChoreDto> createChoreDtoList)
    {
        var result = await _tasksService.CreateChoreListAsync(createChoreDtoList);
        return Ok(result);
    }
    [HttpDelete("delete/{id}")]
    public async Task<ActionResult<ChoreDto>> DeleteChore(string id)
    {
        var result = await _tasksService.DeleteChoreAsync(id);
        return Ok(result);
    }
    public async Task<ActionResult<IEnumerable<ChoreDto>>> GetUsersChores([FromQuery] string? userId)
    {
        if (userId is not null)
        {
            var result = await _tasksService.GetUsersChoreDetailsAsync(userId);
            return Ok(result);
        }
        else
        {
            var myUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (myUserId is null)
                return BadRequest();

            var result = await _tasksService.GetUsersChoreDetailsAsync(myUserId);
            return Ok(result);
        }
    }
    [HttpGet("HouseholdChores")]
    public async Task<ActionResult<IEnumerable<ChoreDto>>> GetMyHouseholdChores(string householdId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
            return BadRequest();

        var result = await _tasksService.GetMyHoueholdChoreDetailsAsync(userId);
        return Ok(result);
    }
    [HttpPost("assign")]
    public async Task<ActionResult<ChoreDto>> AssignChore([FromBody] ChoreDto choreDto, [FromQuery] int? userId)
    {
        if (userId is not null)
        {
            await _tasksService.AssignChoreAsync(choreDto, userId);
            return Ok();
        }
        else
        {
            var myUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (myUserId is null)
                return BadRequest();

            await _tasksService.AssignChoreAsync(choreDto, myUserId);
            return Ok();
        }
    }
    [HttpPost("assignme")]
    public async Task<ActionResult<ChoreDto>> AssignMeChore([FromBody] ChoreDto choreDto)
    {
        await _tasksService.AssignMeChoreAsync(choreDto);
        return Ok();
    }
}
