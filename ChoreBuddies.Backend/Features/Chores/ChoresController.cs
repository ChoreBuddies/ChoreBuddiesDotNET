using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Chores;
using System.Threading.Tasks;

namespace ChoreBuddies.Backend.Features.Chores;

[ApiController]
[Route("api/v1/chores")]
[Authorize]
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
    [HttpDelete("delete/{id}")]
    public async Task<ActionResult<ChoreDto>> DeleteChore(string id)
    {
        var result = await _tasksService.DeleteChoreAsync(id);
        return Ok(result);
    }
}
