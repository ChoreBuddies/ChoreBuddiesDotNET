namespace Shared.Notifications;

public class NotificationPreferenceDto
{
    public NotificationEvent Type { get; set; }
    public NotificationChannel Channel { get; set; }
    public bool IsEnabled { get; set; }
}
