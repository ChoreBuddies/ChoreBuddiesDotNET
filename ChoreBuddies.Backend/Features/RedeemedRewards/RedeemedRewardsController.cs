using ChoreBuddies.Backend.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Rewards;

namespace ChoreBuddies.Backend.Features.RedeemedRewards;
[Route("api/v1/redeemedRewards")]
[ApiController]
[Authorize]
public class RedeemedRewardsController(IRedeemedRewardsService redeemedRewardsService, ITokenService tokenService) : ControllerBase
{
    private readonly IRedeemedRewardsService _redeemedRewardsService = redeemedRewardsService;
    private readonly ITokenService _tokenService = tokenService;
    [HttpPost("redeem")]
    public async Task<ActionResult<RedeemedRewardDto>> RedeemReward([FromQuery] int rewardId, [FromQuery] bool isFulfilled)
    {
        var userId = _tokenService.GetUserIdFromToken(User); 
        var result = await _redeemedRewardsService.RedeemRewardAsync(userId, rewardId, isFulfilled);
        return Ok(result);  
    }
    [HttpGet]
    public async Task<ActionResult<ICollection<RedeemedRewardDto>>> GetUsersRedeemedRewards([FromQuery] int? userId)
    {
        if(userId is null)
        {
            userId = _tokenService.GetUserIdFromToken(User);
        }
        var result = await _redeemedRewardsService.GetUsersRedeemedRewardsAsync((int)userId);
        return Ok(result);
    }
    [HttpGet("household")]
    public async Task<ActionResult<ICollection<RedeemedRewardDto>>> GetHouseholdsRedeemedRewards([FromQuery] int? householdId)
    {
        if (householdId is null)
        {
            householdId = _tokenService.GetHouseholdIdFromToken(User);
        }
        var result = await _redeemedRewardsService.GetHouseholdsRedeemedRewardsAsync((int)householdId);
        return Ok(result);
    }
}
