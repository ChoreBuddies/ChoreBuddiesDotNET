using ChoreBuddies.Backend.Features.Chores;
using ChoreBuddies.Backend.Features.Notifications;
using Hangfire;
using Shared.Reminders;

namespace ChoreBuddies.Backend.Features.Reminders;

public interface IRemindersService
{
    public Task SetReminder(int userId, ReminderDto reminderDto);
}
public class RemindersService(IBackgroundJobClient backgroundJobClient, IChoresService choresService) : IRemindersService
{
    private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;
    private readonly IChoresService _choresService = choresService;

    public async Task SetReminder(int userId, ReminderDto reminderDto)
    {
        var chore = await _choresService.GetChoreDetailsAsync(reminderDto.choreId);
        if (chore == null)
        {
            throw new ArgumentNullException(nameof(chore));
        }
        _backgroundJobClient.Schedule<INotificationService>(
                service => service.SendReminderAsync(userId, chore.Id, chore.Name, default),
                reminderDto.remindAt
            );
    }
}
