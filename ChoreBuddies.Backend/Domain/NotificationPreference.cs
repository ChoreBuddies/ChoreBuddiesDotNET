using Shared.Notifications;

namespace ChoreBuddies.Backend.Domain;

public class NotificationPreference
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public required AppUser User { get; set; }

    public NotificationEvent Type { get; set; }

    public NotificationChannel Channel { get; set; }

    public bool IsEnabled { get; set; }
}

