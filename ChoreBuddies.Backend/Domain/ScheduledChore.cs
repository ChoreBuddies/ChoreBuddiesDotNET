using Shared.ScheduledChores;

namespace ChoreBuddies.Backend.Domain;

public class ScheduledChore
{
    public int Id { get; set; }
    public int MinAge { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? AssignedTo { get; set; }
    public string Room { get; set; } = null!;
    public int RewardPointsCount { get; set; }
    public int HouseholdId { get; set; }
    public int ChoreDuration { get; set; }  // Days
    public Frequency Frequency { get; set; }
    public DateTime? LastGenerated { get; set; }
    public int EveryX { get; set; }
    public Household? Household { get; set; }

    private ScheduledChore() { }

    public ScheduledChore(
        string name,
        string description,
        string? assignedTo,
        string room,
        int everyX,
        Frequency frequency,
        int rewardPointsCount,
        int householdId,
        int minAge = 0,
        int choreDuration = 1
    )
    {
        Name = name;
        Description = description;
        AssignedTo = assignedTo;
        Room = room;
        EveryX = everyX;
        Frequency = frequency;
        RewardPointsCount = rewardPointsCount;
        HouseholdId = householdId;
        MinAge = minAge;
        ChoreDuration = choreDuration;
    }
}
