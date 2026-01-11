using ChoreBuddies.Frontend.Utilities;
using Shared.ScheduledChores;

namespace ChoreBuddies.Frontend.Features.ScheduledChores;

public interface IScheduledChoresService
{
    public Task<IEnumerable<ScheduledChoreTileViewDto>?> GetMyHouseholdChoresAsync();
}
public class ScheduledChoresService(HttpClientUtils httpUtils) : IScheduledChoresService
{
    public async Task<IEnumerable<ScheduledChoreTileViewDto>?> GetMyHouseholdChoresAsync()
    {
        var result = await httpUtils.TryRequestAsync(async () =>
        {
            return await httpUtils.GetAsync<IEnumerable<ScheduledChoreTileViewDto>?>(
                ScheduledChoresConstants.ApiEndpointGetHouseholdScheduledChores,
                authorized: true
            );
        });

        return result ?? [];
    }
}
