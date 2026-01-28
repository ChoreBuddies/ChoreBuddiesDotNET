using ChoreBuddies.Backend.Features.Chores;
using ChoreBuddies.Backend.Features.Notifications;
using ChoreBuddies.Backend.Features.Reminders;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Moq;
using Shared.Chores;
using Shared.Reminders;

namespace ChoreBuddies.Tests.Reminders;

public class RemindersServiceTests
{
    private readonly Mock<IBackgroundJobClient> _backgroundJobClientMock;
    private readonly Mock<IChoresService> _choresServiceMock;
    private readonly RemindersService _service;

    public RemindersServiceTests()
    {
        _backgroundJobClientMock = new Mock<IBackgroundJobClient>();
        _choresServiceMock = new Mock<IChoresService>();

        _service = new RemindersService(
            _backgroundJobClientMock.Object,
            _choresServiceMock.Object
        );
    }

    [Fact]
    public async Task SetReminder_ThrowsArgumentNullException_WhenChoreDoesNotExist()
    {
        // Arrange
        var remindAt = DateTime.Now.AddHours(1);
        var choreId = 10;
        var reminderDto = new ReminderDto(remindAt, choreId);
        _choresServiceMock
            .Setup(x => x.GetChoreDetailsAsync(It.IsAny<int>()))
            .ReturnsAsync((ChoreDto?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.SetReminder(userId: 1, reminderDto)
        );
    }

    [Fact]
    public async Task SetReminder_CreatesHangfireJob_WhenChoreExists()
    {
        // Arrange
        var remindAt = DateTime.Now.AddDays(1);
        var chore = new ChoreDto(10, "Take out trash", "", 0, 0, null, Status.Unassigned, "");
        var reminderDto = new ReminderDto(remindAt, chore.Id);

        _choresServiceMock
            .Setup(x => x.GetChoreDetailsAsync(chore.Id))
            .ReturnsAsync(chore);

        // Act
        await _service.SetReminder(5, reminderDto);

        // Assert
        _backgroundJobClientMock.Verify(
            x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()),
            Times.Once
        );
    }

    [Fact]
    public async Task SetReminder_SchedulesNotificationWithCorrectArguments()
    {
        // Arrange
        var userId = 7;
        var choreId = 3;
        var householdId = 3;
        var remindAt = DateTime.Now.AddHours(1);

        var chore = new ChoreDto(
            choreId,
            "Wash dishes",
            "",
            userId,
            householdId,
            null,
            Status.Unassigned,
            ""
        );
        var reminderDto = new ReminderDto(remindAt, chore.Id);

        _choresServiceMock
            .Setup(x => x.GetChoreDetailsAsync(choreId))
            .ReturnsAsync(chore);

        // Act
        await _service.SetReminder(userId, reminderDto);

        // Assert
        _backgroundJobClientMock.Verify(
            x => x.Create(
                It.Is<Job>(job =>
                    job.Type == typeof(INotificationService) &&
                    job.Method.Name == "SendReminderAsync" &&
                    job.Args[0].Equals(userId) &&
                    job.Args[1].Equals(choreId) &&
                    job.Args[2].Equals(chore.Name)
                ),
                It.Is<IState>(state =>
                    state is ScheduledState &&
                    ((ScheduledState)state).EnqueueAt == remindAt.ToUniversalTime()
                )
            ),
            Times.Once
        );
    }
}
