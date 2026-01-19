using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.DefalutChores;
using Shared.PredefinedRewards;
using Shared.ScheduledChores;

namespace ChoreBuddies.Backend.Features.PredefinedRewards;

[Route("api/v1/predefinedRewards")]
[ApiController]
[Authorize]
public class PredefinedRewardsController(IPredefinedRewardsService predefinedRewardsService) : ControllerBase
{
    private readonly IPredefinedRewardsService _service = predefinedRewardsService;
    [HttpGet("all")]
    public async Task<ActionResult<ICollection<PredefinedRewardDto>>> GetAllPredefinedRewards()
    {
        var result = await _service.GetAllPredefinedRewardsAsync();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<List<PredefinedRewardDto>>> GetPredefinedRewards(PredefinedRewardRequest request)
    {
        var rewards = await _service.GetPredefinedRewardsAsync(request.PredefinedRewardIds);
        return Ok(rewards);
    }
}
