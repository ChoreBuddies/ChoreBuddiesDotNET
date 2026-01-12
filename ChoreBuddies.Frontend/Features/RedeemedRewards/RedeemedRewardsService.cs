using ChoreBuddies.Frontend.Utilities;
using Shared.RedeemedRewards;

namespace ChoreBuddies.Frontend.Features.RedeemedRewards;

public interface IRedeemedRewardsService
{
    public Task<RedeemedRewardDto?> RedeemRewardAsync(int rewardId, bool isfulfilled = false);
    public Task<bool> FulfillRewardAsync(int rewardId);
    public Task<ICollection<RedeemedRewardDto>> GetUsersRedeemedRewardsAsync();
    public Task<ICollection<RedeemedRewardWithUserNameDto>> GetHouseholdsUnfulfilledRedeemedRewardsAsync();
}
public class RedeemedRewardsService(HttpClientUtils httpUtils) : IRedeemedRewardsService
{
    public async Task<bool> FulfillRewardAsync(int redeemedRewardId)
    {
        return await httpUtils.TryRequestAsync(async () =>
        {
            return await httpUtils.PutAsync<int, bool>(
                RedeemedRewardsConstants.ApiEndpointFulfillReward + redeemedRewardId,
                redeemedRewardId,
                authorized: true
            );
        });
    }

    public async Task<RedeemedRewardDto?> RedeemRewardAsync(int rewardId, bool isFulfilled = false)
    {
        return await httpUtils.TryRequestAsync(async () =>
        {
            return await httpUtils.PostAsync<bool,RedeemedRewardDto>(
                $"{RedeemedRewardsConstants.ApiEndpointRedeemRewards}{rewardId}", 
                isFulfilled,
                authorized: true
            );
        });
    }

    public async Task<ICollection<RedeemedRewardDto>> GetUsersRedeemedRewardsAsync()
    {
        return await httpUtils.TryRequestAsync(async () =>
        {
            return await httpUtils.GetAsync<ICollection<RedeemedRewardDto>>(
                RedeemedRewardsConstants.ApiEndpointGetUsersRedeemedRewards,
                authorized: true
            );
        }) ?? [];
    }

    public async Task<ICollection<RedeemedRewardWithUserNameDto>> GetHouseholdsUnfulfilledRedeemedRewardsAsync()
    {
        return await httpUtils.TryRequestAsync(async () =>
        {
            return await httpUtils.GetAsync<ICollection<RedeemedRewardWithUserNameDto>>(
                RedeemedRewardsConstants.ApiEndpointGetHouseholdsUnfulfilledRedeemedRewards,
                authorized: true
            );
        }) ?? [];
    }
}
