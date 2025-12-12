using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DefalutChores;

namespace ChoreBuddies.Backend.Features.DefaultChores;

[ApiController]
[Route("api/v1/predefinedChores")]
[Authorize]
public class PredefinedChoreController(IPredefinedChoreService service, IMapper mapper) : Controller
{
    private IPredefinedChoreService _service = service;
    private IMapper _mapper = mapper;

    [HttpGet("all")]
    public async Task<IActionResult> GetAllPredefinedChores()
    {
        var chores = await _service.GetAllPredefinedChoresAsync();
        return Ok(_mapper.Map<List<PredefinedChoreDto>>(chores));
    }
}
