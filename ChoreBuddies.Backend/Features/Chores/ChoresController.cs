using ChoreBuddies.Backend.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Chores;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ChoreBuddies.Backend.Features.Chores;

[ApiController]
[Route("api/v1/chores")]
[Authorize]
public class ChoresController : ControllerBase
{
    private readonly IChoresService _choresService;
    private readonly ITokenService _tokenService;

    public ChoresController(IChoresService tasksService, ITokenService tokenService)
    {
        _choresService = tasksService;
        _tokenService = tokenService; // TODO: update methods to use tokenservice
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ChoreDto>> GetChore(int id)
    {
        var result = await _choresService.GetChoreDetailsAsync(id);
        return Ok(result);
    }
    [HttpPost("update")]
    public async Task<ActionResult<ChoreDto>> UpdateChore([FromBody] ChoreDto choreDto)
    {
        var result = await _choresService.UpdateChoreAsync(choreDto);
        return Ok(result);
    }
    [HttpPost("add")]
    public async Task<ActionResult<ChoreDto>> AddChore([FromBody] CreateChoreDto createChoreDto)
    {
        if (createChoreDto.HouseholdId <= 0)
        {
            createChoreDto = createChoreDto with { HouseholdId = _tokenService.GetHouseholdIdFromToken(User) };
        }
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
    public async Task<ActionResult<ChoreDto>> DeleteChore(int id)
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
    public async Task<ActionResult<IEnumerable<ChoreDto>>> GetMyHouseholdChores()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
            return Unauthorized();

        var result = await _choresService.GetMyHouseholdChoreDetailsAsync(int.Parse(userId));
        return Ok(result);
    }
    [HttpPost("assign")]
    public async Task<ActionResult> AssignChore([FromBody] ChoreDto choreDto, [FromQuery] int? userId)
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
    [HttpPost("markAsDone")]
    public async Task<ActionResult<ChoreDto>> MarkChoreAsDone([FromQuery] int choreId)
    {
        var userId = _tokenService.GetUserIdFromToken(User);
        var result = await _choresService.MarkChoreAsDone(choreId, userId);
        return Ok(result);
    }
}
