using ChoreBuddies.Frontend.Utilities;
using Shared.ScheduledChores;

namespace ChoreBuddies.Frontend.Features.ScheduledChores;

public interface IScheduledChoresService
{
    Task<IEnumerable<ScheduledChoreTileViewDto>?> GetMyHouseholdChoresAsync();
    Task<ScheduledChoreTileViewDto?> UpdateChoreFrequencyAsync(int choreId, Frequency frequency);
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
}
