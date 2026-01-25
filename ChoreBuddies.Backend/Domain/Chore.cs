using Shared.Chores;

namespace ChoreBuddies.Backend.Domain;

public class Chore(
    string name,
    string description,
    int? userId,
    int householdId,
    DateTime? dueDate,
    Status? status,
    string room,
    int rewardPointsCount,
    DateTime? completedDate)
{

    public int Id { get; set; } = 0;
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public int? UserId { get; set; } = userId;
    public int HouseholdId { get; set; } = householdId;
    public DateTime? DueDate { get; set; } = dueDate;
    public Status? Status { get; set; } = status;
    public string Room { get; set; } = room;
    public int RewardPointsCount { get; set; } = rewardPointsCount;
    public DateTime? CompletedDate { get; set; } = completedDate;
    public DateTime? LastEditDate { get; set; }

    // Navigation properties
    public virtual Household? Household { get; set; }
    public virtual AppUser? User { get; set; }
    private Chore() : this(
    default!, // name
    default!, // description
    default,  // userId
    default,  // householdId
    default,  // dueDate
    default,  // status
    default!, // room
    default,  // rewardPointsCount
    default   // completedDate
    ) { }
}
