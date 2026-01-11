using ChoreBuddies.Frontend.Utilities;
using Shared.Rewards;

namespace ChoreBuddies.Frontend.Features.Rewards;

public interface IRewardsService
{
    public Task<ICollection<RewardDto>> GetHouseholdRewardsAsync();
}
public class RewardsService(HttpClientUtils httpUtils) : IRewardsService
{
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
