using ChoreBuddies.Frontend.Features.PredefinedChores;
using ChoreBuddies.Frontend.Utilities;
using Shared.PredefinedChores;
using Shared.PredefinedChores;
using Shared.PredefinedRewards;

namespace ChoreBuddies.Frontend.Features.PredefinedRewards;

public interface IPredefinedRewardsService
{
    Task<IEnumerable<PredefinedRewardDto>> GetAllPredefinedRewardsAsync();
    Task<IEnumerable<PredefinedRewardDto>> GetPredefinedRewardsAsync(List<int> predefinedRewardIds);
}

public class PredefinedRewardsService(HttpClientUtils httpUtils) : IPredefinedRewardsService
{
    public async Task<IEnumerable<PredefinedRewardDto>> GetAllPredefinedRewardsAsync()
    {
        var result = await httpUtils.GetAsync<List<PredefinedRewardDto>>(
                PredefinedRewardsConstants.ApiEndpointGetAll,
                authorized: true
            );

        return result ?? [];
    }

    public async Task<IEnumerable<PredefinedRewardDto>> GetPredefinedRewardsAsync(List<int> predefinedRewardIds)
    {
        var result = await httpUtils.PostAsync<PredefinedRewardRequest, List<PredefinedRewardDto>>(
                PredefinedRewardsConstants.ApiEndpointGet,
                new PredefinedRewardRequest { PredefinedRewardIds = predefinedRewardIds },
                authorized: true
            );

        return result ?? [];
    }
}
