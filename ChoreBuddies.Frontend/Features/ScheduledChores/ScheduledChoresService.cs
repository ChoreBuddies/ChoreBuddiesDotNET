using ChoreBuddies.Frontend.Utilities;
using Shared.ScheduledChores;

namespace ChoreBuddies.Frontend.Features.ScheduledChores;

public interface IScheduledChoresService
{
    Task<ScheduledChoreDto?> GetChoreByIdAsync(int id);
    Task<IEnumerable<ScheduledChoreTileViewDto>?> GetMyHouseholdChoresAsync();
    Task<ScheduledChoreTileViewDto?> UpdateChoreFrequencyAsync(int choreId, Frequency frequency);
    Task<ScheduledChoreDto?> UpdateChoreAsync(ScheduledChoreDto choreDto);
    Task<ScheduledChoreDto?> CreateChoreAsync(CreateScheduledChoreDto choreDto);

    Task<bool> DeleteChoreAsync(int choreId);

}
public class ScheduledChoresService(HttpClientUtils httpUtils) : IScheduledChoresService
{
    public async Task<bool> DeleteChoreAsync(int choreId)
    {
        try
        {
            var result = await httpUtils.DeleteAsync(
               $"{ScheduledChoresConstants.ApiEndpointScheduledChores}/{choreId}",
               authorized: true
           );
            return result.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<ScheduledChoreDto?> GetChoreByIdAsync(int id)
    {
        return await httpUtils.GetAsync<ScheduledChoreDto?>(
    $"{ScheduledChoresConstants.ApiEndpointScheduledChores}/{id}",
    authorized: true
);
    }

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

    public async Task<ScheduledChoreDto?> UpdateChoreAsync(ScheduledChoreDto choreDto)
    {
        return await httpUtils.PostAsync<ScheduledChoreDto, ScheduledChoreDto?>(
            ScheduledChoresConstants.ApiEndpointUpdateScheduledChores,
            choreDto,
            authorized: true
        );
    }
    public async Task<ScheduledChoreDto?> CreateChoreAsync(CreateScheduledChoreDto choreDto)
    {
        return await httpUtils.PostAsync<CreateScheduledChoreDto, ScheduledChoreDto?>(
            ScheduledChoresConstants.ApiEndpointCreateScheduledChores,
            choreDto,
            authorized: true
        );
    }

    public async Task<ScheduledChoreTileViewDto?> UpdateChoreFrequencyAsync(int choreId, Frequency frequency)
    {
        var dto = new ScheduledChoreFrequencyUpdateDto
        {
            Id = choreId,
            Frequency = frequency
        };

        return await httpUtils.PutAsync<ScheduledChoreFrequencyUpdateDto, ScheduledChoreTileViewDto>(
            ScheduledChoresConstants.ApiEndpointUpdateChoreFrequency,
            dto,
            authorized: true
        );
    }

    public async Task<ScheduledChoreDto?> UpdateChoreFrequencyAsync(ScheduledChoreDto choreDto)
    {
        return await httpUtils.PostAsync<ScheduledChoreDto, ScheduledChoreDto?>(
            ScheduledChoresConstants.ApiEndpointScheduledChores,
            choreDto,
            authorized: true
        );
    }
}
