using ChoreBuddies.Backend.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Authentication;
using Shared.RedeemedRewards;

namespace ChoreBuddies.Backend.Features.RedeemedRewards;

[Route("api/v1/redeemedRewards")]
[ApiController]
[Authorize]
public class RedeemedRewardsController(IRedeemedRewardsService redeemedRewardsService, ITokenService tokenService) : ControllerBase
{
    private readonly IRedeemedRewardsService _redeemedRewardsService = redeemedRewardsService;
    private readonly ITokenService _tokenService = tokenService;

    [HttpPost("redeem")]
    public async Task<ActionResult<RedeemedRewardDto>> RedeemReward([FromBody] CreateRedeemedRewardDto rewardDto)
    {
        var userId = _tokenService.GetUserIdFromToken(User);
        var role = _tokenService.GetUserRoleFromToken(User);
        var result = await _redeemedRewardsService.RedeemRewardAsync(userId, rewardDto.RewardId, rewardDto.IsFulfilled && role == "Adult");
        return Ok(result);
    }

    [HttpPut("fulfill")]
    [Authorize(Roles = AuthConstants.RoleAdult)]
    public async Task<ActionResult<bool>> FulfillReward([FromBody] FulfillRewardDto fulfillRewardDto)
    {
        var role = _tokenService.GetUserRoleFromToken(User);
        if (role != "Adult")
        {
            return Forbid();
        }
        return Ok(await _redeemedRewardsService.FulfillRewardAsync(fulfillRewardDto.redeemedRewardId));
    }

    [HttpGet]
    public async Task<ActionResult<ICollection<RedeemedRewardDto>>> GetUsersRedeemedRewards([FromQuery] int? userId)
    {
        if (userId is null)
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
    [HttpGet("household/unfulfilled")]
    public async Task<ActionResult<ICollection<RedeemedRewardWithUserNameDto>>> GetHouseholdsUnfulfilledRedeemedRewards([FromQuery] int? householdId)
    {
        if (householdId is null)
        {
            householdId = _tokenService.GetHouseholdIdFromToken(User);
        }
        var result = await _redeemedRewardsService.GetHouseholdsUnfulfilledRedeemedRewardsAsync((int)householdId);
        return Ok(result);
    }
}
