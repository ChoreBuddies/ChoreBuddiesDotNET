using Shared.Chores;

namespace ChoreBuddies.Backend.Domain;

public class Chore(
    string name,
    string description,
    string? assignedTo,
    DateTime? dueDate,
    Status? status,
    string room,
    int rewardPointsCount) // TODO: Add household to constructor
{
    public Chore(string id, string name, string description, string? assignedTo, DateTime? dueDate, Status? status, string room, int rewardPointsCount, int householdId, Household? household) : this(name, description, assignedTo, dueDate, status, room, rewardPointsCount)
    {
        Id = id;
    }

    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public string? AssignedTo { get; set; } = assignedTo;
    public DateTime? DueDate { get; set; } = dueDate;
    public Status? Status { get; set; } = status;
    public string Room { get; set; } = room;
    public int RewardPointsCount { get; set; } = rewardPointsCount;

    public int HouseholdId { get; set; } = 1;

    // Navigation properties
    public Household? Household { get; set; } = null;
}
