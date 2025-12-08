using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Rewards;

namespace ChoreBuddies.Backend.Features.DefaultRewards;
[Route("api/v1/defaultRewards")]
[ApiController]
[Authorize]
public class DefaultRewardsController(IDefaultRewardsService defaultRewardsService) : ControllerBase
{
    private readonly IDefaultRewardsService _defaultRewardsService = defaultRewardsService;
    [HttpGet("all")]
    public async Task<ActionResult<ICollection<DefaultRewardDto>>> GetAllDefaultRewards()
    {
        var result = await _defaultRewardsService.GetAllDefaultRewardsAsync();
        return Ok(result);
    }
}
