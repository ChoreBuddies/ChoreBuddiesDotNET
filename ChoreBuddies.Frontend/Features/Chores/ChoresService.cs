using ChoreBuddies.Frontend.Utilities;
using Shared.Chat;
using Shared.Chores;
using System.Net.Http.Json;

namespace ChoreBuddies.Frontend.Features.Chores;

public interface IChoresService
{
    public Task<ChoreDto?> CreateChoreAsync(CreateChoreDto createChoreDto);
    public Task<ChoreDto?> GetChoreByIdAsync(int id);
    public Task<IEnumerable<ChoreDto>?> GetMyChoresAsync();
    public Task<IEnumerable<ChoreDto>?> GetMyHouseholdChoresAsync();
    public Task<ChoreDto?> UpdateChoreAsync(ChoreDto choreDto);
}
public class ChoresService(HttpClientUtils httpUtils) : IChoresService
{
    public async Task<ChoreDto?> CreateChoreAsync(CreateChoreDto createChoreDto)
    {
        var result = await httpUtils.TryRequestAsync(async () =>
        {
            return await httpUtils.PostAsync<CreateChoreDto, ChoreDto?>(
                ChoresConstants.ApiEndpointCreateChore,
                createChoreDto,
                authorized: true
            );
        });
        return result ?? null;
    }
    public async Task<ChoreDto?> GetChoreByIdAsync(int id)
    {
        var result = await httpUtils.TryRequestAsync(async () =>
        {
            return await httpUtils.GetAsync<ChoreDto?>(
                $"{ChoresConstants.ApiEndpointGetChoreById}{id}",
                authorized: true
            );
        });

        return result ?? null;
    }
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
