using Microsoft.AspNetCore.Identity;

namespace ChoreBuddies.Backend.Domain;

public class AppUser : IdentityUser<int>
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public DateTime DateOfBirth { get; set; }

    public int? HouseholdId { get; set; }

    public int PointsCount { get; set; } = 0;

    // Refresh Token Properties
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiry { get; set; }

    // Navigation properties
    public Household? Household { get; set; }

    public virtual ICollection<NotificationPreference> NotificationPreferences { get; set; }
        = new List<NotificationPreference>();
    public virtual ICollection<Chore> Chores { get; set; } = [];
    public virtual ICollection<RedeemedReward> RedeemedRewards { get; set; } = [];
}
