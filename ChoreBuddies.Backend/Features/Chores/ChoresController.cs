using Microsoft.AspNetCore.Mvc;
using Shared.Chores;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ChoreBuddies.Backend.Features.Chores;

[ApiController]
[Route("api/v1/chores")]
public class ChoresController : ControllerBase
{
    private readonly IChoresService _choresService;

    public ChoresController(IChoresService tasksService)
    {
        _choresService = tasksService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ChoreDto>> GetChore(string id)
    {
        var result = await _choresService.GetChoreDetailsAsync(id);
        return Ok(result);
    }
    [HttpPost("update")]
    public async Task<ActionResult<ChoreDto>> UpdadeChore([FromBody] ChoreDto choreDto)
    {
        var result = await _choresService.UpdateChoreAsync(choreDto);
        return Ok(result);
    }
    [HttpPost("add")]
    public async Task<ActionResult<ChoreDto>> AddChore([FromBody] CreateChoreDto createChoreDto)
    {
        var result = await _choresService.CreateChoreAsync(createChoreDto);
        return Ok(result);
    }
    [HttpPost("addlist")]
    public async Task<ActionResult<IEnumerable<ChoreDto>>> AddChoreList([FromBody] IEnumerable<CreateChoreDto> createChoreDtoList)
    {
        var result = await _choresService.CreateChoreListAsync(createChoreDtoList);
        return Ok(result);
    }
    [HttpDelete("delete/{id}")]
    public async Task<ActionResult<ChoreDto>> DeleteChore(string id)
    {
        var result = await _choresService.DeleteChoreAsync(id);
        return Ok(result);
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChoreDto>>> GetUsersChores([FromQuery] int? userId)
    {
        if (userId is not null)
        {
            var result = await _choresService.GetUsersChoreDetailsAsync((int)userId);
            return Ok(result);
        }
        else
        {
            var myUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (myUserId is null)
                return Unauthorized();

            var result = await _choresService.GetUsersChoreDetailsAsync(int.Parse(myUserId));
            return Ok(result);
        }
    }
    [HttpGet("HouseholdChores")]
    public async Task<ActionResult<IEnumerable<ChoreDto>>> GetMyHouseholdChores(string householdId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
            return Unauthorized();

        var result = await _choresService.GetMyHouseholdChoreDetailsAsync(int.Parse(userId));
        return Ok(result);
    }
    [HttpPost("assign")]
    public async Task<ActionResult<ChoreDto>> AssignChore([FromBody] ChoreDto choreDto, [FromQuery] int? userId)
    {
        if (userId is not null)
        {
            await _choresService.AssignChoreAsync(choreDto, (int)userId);
            return Ok();
        }
        else
        {
            var myUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (myUserId is null)
                return Unauthorized();

            await _choresService.AssignChoreAsync(choreDto, int.Parse(myUserId));
            return Ok();
        }
    }
}
