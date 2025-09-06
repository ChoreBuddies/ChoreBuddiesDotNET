using ChoreBuddies_SharedModels.Chores;

namespace ChoreBuddies.Backend.Domain;

public class Chore(
    string id,
    string name,
    string description,
    string? assignedTo,
    DateTime dueDate,
    Status status,
    string room,
    int rewardPointsCount)
{
    public string Id { get; set; } = id;
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public string? AssignedTo { get; set; } = assignedTo;
    public DateTime DueDate { get; set; } = dueDate;
    public Status Status { get; set; } = status;
    public string Room { get; set; } = room;
    public int RewardPointsCount { get; set; } = rewardPointsCount;

    public int HouseholdId { get; set; }

    // Navigation properties
    public Household Household { get; set; }
}
