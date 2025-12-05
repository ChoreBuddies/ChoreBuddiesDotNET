namespace Shared.Notifications;
public enum NotificationEvent
{
    NewChore = 1,
    RewardRequest = 2,
    ChoreCompleted = 3
}

public enum NotificationChannel
{
    Email = 1,
    Push = 2,
    InApp = 3
}
