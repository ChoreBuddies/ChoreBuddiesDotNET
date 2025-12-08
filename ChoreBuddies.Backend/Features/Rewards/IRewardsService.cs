using ChoreBuddies.Backend.Domain;
using Shared.Rewards;

namespace ChoreBuddies.Backend.Features.Rewards;

public interface IRewardsService
{
    // Create
    public Task<RewardDto?> CreateRewardAsync(CreateRewardDto createRewardDto);
    // Read
    public Task<RewardDto?> GetRewardByIdAsync(int rewardId);
    // Update
    public Task<RewardDto?> UpdateRewardAsync(RewardDto rewardDto);
    // Delete
    public Task<RewardDto?> DeleteRewardAsync(int rewardId);
    // Get Household's Rewards
    public Task<ICollection<RewardDto>?> GetHouseholdRewardsAsync(int householdId);
}
