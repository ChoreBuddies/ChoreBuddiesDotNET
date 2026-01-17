using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DefalutChores;
using Shared.ScheduledChores;

namespace ChoreBuddies.Backend.Features.DefaultChores;

[ApiController]
[Route("api/v1/predefinedChores")]
[Authorize]
public class PredefinedChoreController(IPredefinedChoreService service, IMapper mapper) : Controller
{
    private IPredefinedChoreService _service = service;
    private IMapper _mapper = mapper;

    [HttpGet("all")]
    public async Task<ActionResult<List<PredefinedChoreDto>>> GetAllPredefinedChores()
    {
        var chores = await _service.GetAllPredefinedChoresAsync();
        return Ok(_mapper.Map<List<PredefinedChoreDto>>(chores));
    }

    [HttpPost]
    public async Task<ActionResult<List<PredefinedChoreDto>>> GetPredefinedChores(PredefinedChoreIdsRequest request)
    {
        var chores = await _service.GetPredefinedChoresAsync(request.PredefinedChoreIds);
        return Ok(_mapper.Map<List<PredefinedChoreDto>>(chores));
    }
}
