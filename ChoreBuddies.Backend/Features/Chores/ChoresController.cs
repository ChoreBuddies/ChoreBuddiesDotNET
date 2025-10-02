using Microsoft.AspNetCore.Mvc;
using Shared.Chores;

namespace ChoreBuddies.Backend.Features.Chores;

[ApiController]
[Route("api/v1/[controller]")]
public class ChoresController : ControllerBase
{
    private readonly IChoresService _tasksService;

    public ChoresController(IChoresService tasksService)
    {
        _tasksService = tasksService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<ChoreOverviewDto>> GetChores()
    {
        return Ok(_tasksService.GetChores());
    }
    [HttpGet("{id}")]
    public ActionResult<IEnumerable<ChoreDto>> GetChore(string id)
    {
        return Ok(_tasksService.GetChoreDetails(id));
    }
}
