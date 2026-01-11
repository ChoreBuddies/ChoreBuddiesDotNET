using ChoreBuddies.Frontend.Utilities;
using Shared.Rewards;

namespace ChoreBuddies.Frontend.Features.Rewards;

public interface IRewardsService
{
    public Task<RewardDto?> GetRewardByIdAsync(int id);
    public Task<RewardDto?> AddRewardAsync(CreateRewardDto createRewardDto);
    public Task<RewardDto?> UpdateRewardAsync(RewardDto updateRewardDto);
    public Task<ICollection<RewardDto>> GetHouseholdRewardsAsync();
}
public class RewardsService(HttpClientUtils httpUtils) : IRewardsService
{
    public async Task<RewardDto?> GetRewardByIdAsync(int id)
    {
        return await httpUtils.TryRequestAsync(async () =>
        {
            return await httpUtils.GetAsync<RewardDto?>(
                RewardsConstants.ApiEndpointGetRewardById + id,
                authorized: true
            );
        });
    }

    public async Task<RewardDto?> AddRewardAsync(CreateRewardDto createRewardDto)
    {
        return await httpUtils.TryRequestAsync(async () =>
        {
            return await httpUtils.PostAsync<CreateRewardDto, RewardDto?>(
                RewardsConstants.ApiEndpointAddReward,
                createRewardDto,
                authorized: true
            );
        });
    }
    public async Task<RewardDto?> UpdateRewardAsync(RewardDto updateRewardDto)
    {
        return await httpUtils.TryRequestAsync(async () =>
        {
            return await httpUtils.PostAsync<RewardDto, RewardDto?>(
                RewardsConstants.ApiEndpointUpdateReward,
                updateRewardDto,
                authorized: true
            );
        });
    }

    public async Task<ICollection<RewardDto>> GetHouseholdRewardsAsync()
    {
        var result = await httpUtils.TryRequestAsync(async () =>
        {
            return await httpUtils.GetAsync<ICollection<RewardDto>>(
                RewardsConstants.ApiEndpointGetHouseholdRewards,
                authorized: true
            );
        });

        return result ?? [];
    }
}
