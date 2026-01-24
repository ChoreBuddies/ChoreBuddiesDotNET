using ChoreBuddies.Frontend.Utilities;
using Shared.Reminders;

namespace ChoreBuddies.Frontend.Features.Reminders;

public interface IRemindersService
{
    public Task<bool> SetReminder(int choreId, DateTime remindAt);
}

public class RemindersService : IRemindersService
{
    private readonly HttpClientUtils _httpClientUtils;

    public RemindersService(HttpClientUtils httpClientUtils)
    {
        _httpClientUtils = httpClientUtils;
    }

    public async Task<bool> SetReminder(int choreId, DateTime remindAt)
    {
        var data = new ReminderDto(remindAt, choreId);
        return await _httpClientUtils.TryRequestAsync(async () =>
        {
            await _httpClientUtils.PostAsync(RemindersConstants.ApiEndpointSetReminder, data, true);
            return true;
        });
    }
}
