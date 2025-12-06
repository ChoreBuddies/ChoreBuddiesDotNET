using ChoreBuddies.Frontend.Utilities;
using Shared.Chat;
using Shared.Chores;
using System.Net.Http.Json;

namespace ChoreBuddies.Frontend.Features.Chores;

public interface IChoresService
{
    public Task<IEnumerable<ChoreDto>?> GetMyChoresAsync();
    public Task<IEnumerable<ChoreDto>?> GetMyHouseholdChoresAsync();
    public Task<ChoreDto?> UpdateChoreAsync(ChoreDto choreDto);
}
public class ChoresService(HttpClientUtils httpUtils) : IChoresService
{
    public async Task<IEnumerable<ChoreDto>?> GetMyChoresAsync()
    {
        var result = await httpUtils.TryRequestAsync(async () =>
        {
            return await httpUtils.GetAsync<IEnumerable<ChoreDto>?>(
                ChoresConstants.ApiEndpointGetUsersChores,
                authorized: true
            );
        });

        return result ?? [];
    }

    public async Task<IEnumerable<ChoreDto>?> GetMyHouseholdChoresAsync()
    {
        var result = await httpUtils.TryRequestAsync(async () =>
        {
            return await httpUtils.GetAsync<IEnumerable<ChoreDto>?>(
                ChoresConstants.ApiEndpointGetHouseholdChores,
                authorized: true
            );
        });

        return result ?? [];
    }

    public async Task<ChoreDto?> UpdateChoreAsync(ChoreDto choreDto)
    {
        var result = await httpUtils.TryRequestAsync(async () =>
        {
            return await httpUtils.PostAsync<ChoreDto, ChoreDto?>(
                ChoresConstants.ApiEndpointUpdateChore,
                choreDto,
                authorized: true
            );
        });
        return result;
    }
}
