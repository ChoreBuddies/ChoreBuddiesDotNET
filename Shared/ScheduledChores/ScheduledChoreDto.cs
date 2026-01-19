namespace Shared.PredefinedChores;

public record ScheduledChoreDto
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public int? UserId { get; init; }
    public string Room { get; init; } = "";
    public int RewardPointsCount { get; init; }
    public int HouseholdId { get; init; }
    public int ChoreDuration { get; init; }   // Days
    public Frequency Frequency { get; init; }
    public DateTime? LastGenerated { get; init; }
    public int EveryX { get; init; }
}
