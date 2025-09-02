using ChoreBuddies_SharedModels.Chores;
using Microsoft.AspNetCore.Mvc;

namespace ChoreBuddies.Backend.Features.Chores
{
    [ApiController]
    [Route("api/[controller]")]
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
        [HttpGet("/{id}")]
        public ActionResult<IEnumerable<ChoreDto>> GetChore(string id)
        {
            return Ok(_tasksService.GetChoreDetails(id));
        }
    }
}