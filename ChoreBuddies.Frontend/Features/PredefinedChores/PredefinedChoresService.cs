using ChoreBuddies.Frontend.Utilities;
using Shared.Chat;
using Shared.PredefinedChores;
using Shared.PredefinedChores;

namespace ChoreBuddies.Frontend.Features.PredefinedChores;

public interface IPredefinedChoresService
{
    Task<IEnumerable<PredefinedChoreDto>> GetAllPredefinedChoresAsync();
    Task<IEnumerable<PredefinedChoreDto>> GetPredefinedChoresAsync(List<int> predefinedChoreIds);
}

public class PredefinedChoresService(HttpClientUtils httpUtils) : IPredefinedChoresService
{
    public async Task<IEnumerable<PredefinedChoreDto>> GetAllPredefinedChoresAsync()
    {
        var result = await httpUtils.GetAsync<List<PredefinedChoreDto>>(
                PredefinedChoresConstants.ApiEndpointGetAll,
                authorized: true
            );

        return result ?? [];
    }

    public async Task<IEnumerable<PredefinedChoreDto>> GetPredefinedChoresAsync(List<int> predefinedChoreIds)
    {
        var result = await httpUtils.PostAsync<PredefinedChoreRequest, List<PredefinedChoreDto>>(
                PredefinedChoresConstants.ApiEndpointGet,
                new PredefinedChoreRequest { PredefinedChoreIds = predefinedChoreIds },
                authorized: true
            );

        return result ?? [];
    }
}
