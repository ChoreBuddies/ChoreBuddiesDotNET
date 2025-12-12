using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.PredefinedRewards;

namespace ChoreBuddies.Backend.Features.PredefinedRewards;

[Route("api/v1/predefinedRewards")]
[ApiController]
[Authorize]
public class PredefinedRewardsController(IPredefinedRewardsService predefinedRewardsService) : ControllerBase
{
    private readonly IPredefinedRewardsService _predefinedRewardsService = predefinedRewardsService;
    [HttpGet("all")]
    public async Task<ActionResult<ICollection<PredefinedRewardDto>>> GetAllPredefinedRewards()
    {
        var result = await _predefinedRewardsService.GetAllPredefinedRewardsAsync();
        return Ok(result);
    }
}
