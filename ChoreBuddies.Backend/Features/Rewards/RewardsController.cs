using ChoreBuddies.Backend.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.PredefinedChores;
using Shared.PredefinedRewards;
using Shared.Rewards;

namespace ChoreBuddies.Backend.Features.Rewards;

[Route("api/v1/rewards")]
[ApiController]
[Authorize]
public class RewardsController : ControllerBase
{
    private readonly IRewardsService _rewardsService;
    private readonly ITokenService _tokenService;
    public RewardsController(IRewardsService rewardsService, ITokenService tokenService)
    {
        _rewardsService = rewardsService;
        _tokenService = tokenService;
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<RewardDto>> GetReward(int id)
    {
        var result = await _rewardsService.GetRewardByIdAsync(id);
        return Ok(result);
    }

    [HttpPost("update")]
    public async Task<ActionResult<RewardDto>> UpdateReward([FromBody] RewardDto rewardDto)
    {
        var result = await _rewardsService.UpdateRewardAsync(rewardDto);
        return Ok(result);
    }

    [HttpPost("add")]
    public async Task<ActionResult<RewardDto>> AddReward([FromBody] CreateRewardDto createRewardDto)
    {
        var result = await _rewardsService.CreateRewardAsync(createRewardDto);
        return Ok(result);
    }

    [HttpPost("add-predefined")]
    public async Task<ActionResult<IEnumerable<RewardDto>>> AddPredefinedRewards([FromBody] PredefinedRewardRequest request)
    {
        var householdId = _tokenService.GetHouseholdIdFromToken(User);
        var result = await _rewardsService.AddPredefinedRewardsToHouseholdAsync(request.PredefinedRewardIds, householdId);
        return Ok(result);
    }

    [HttpDelete("delete")]
    public async Task<ActionResult<RewardDto>> DeleteReward([FromQuery] int id)
    {
        var result = await _rewardsService.DeleteRewardAsync(id);
        return Ok(result);
    }

    [HttpGet("householdRewards")]
    public async Task<ActionResult<ICollection<RewardDto>>> GetMyHouseholdRewards([FromQuery] int? householdId)
    {
        if (householdId == null)
        {
            householdId = _tokenService.GetHouseholdIdFromToken(User);
        }
        var result = await _rewardsService.GetHouseholdRewardsAsync((int)householdId);
        return Ok(result);
    }
}
