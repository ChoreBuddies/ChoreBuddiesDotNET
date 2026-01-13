using Shared.ScheduledChores;
using System.ComponentModel.DataAnnotations;

namespace ChoreBuddies.Frontend.Features.Chores;

public class ChoreViewModel
{
    [Required(ErrorMessage = "Name Is Required")]
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? UserId { get; set; }
    public int RewardPointsCount { get; set; }
    public DateTime? DueDate { get; set; }
    public int HouseholdId { get; set; }
    public bool IsScheduled { get; set; }
    public int ChoreDuration { get; set; } = 1;
    public int EveryX { get; set; } = 1;
    public Frequency Frequency { get; set; } = Frequency.Daily;
}
