using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ChoreBuddies.Backend.Domain;

public class Household(int ownerId, string name, string invitationCode, string? description)
{
    public int Id { get; set; }
    public int OwnerId { get; set; } = ownerId;

    [MaxLength(50, ErrorMessage = "Household name must be 1-50 characters"), MinLength(1)]
    public string Name { get; set; } = name;

    [StringLength(6, MinimumLength = 6, ErrorMessage = "Invitation code must be 6 characters long")]
    public string InvitationCode { get; set; } = invitationCode;

    [MaxLength(250, ErrorMessage = "Household description must be less than 250 characters")]
    public string? Description { get; set; } = description;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties

    [JsonIgnore]
    public virtual ICollection<AppUser> Users { get; set; } = []; // TODO Change to HashSet<AppUser>

    [JsonIgnore]
    public virtual ICollection<Chore> Chores { get; set; } = [];
    [JsonIgnore]
    public virtual ICollection<Reward> Rewards { get; set; } = [];
    [JsonIgnore]
    public virtual ICollection<RedeemedReward> RedeemedRewards { get; set; } = [];
    public virtual ICollection<ScheduledChore> ScheaduledChores { get; set; } = [];

}

