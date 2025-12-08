using ChoreBuddies.Backend.Domain;
using Shared.Rewards;

namespace ChoreBuddies.Backend.Features.RedeemedRewards;

public interface IRedeemedRewardsService
{
    // Redeem
    public Task<RedeemedRewardDto?> RedeemRewardAsync(int userId, int rewardId, bool isFulfilled);
    // Get User's Redeemed
    public Task<ICollection<RedeemedRewardDto>> GetUsersRedeemedRewardsAsync(int userId);
    // Get Household's Redeemed
    public Task<ICollection<RedeemedRewardDto>> GetHouseholdsRedeemedRewardsAsync(int householdId);
}
