namespace Shared.ScheduledChores;

public record CreateScheduledChoreDto(
    string Name,
    string Description,
    string? AssignedTo,
    string Room,
    int RewardPointsCount,
    Frequency frequency,
    int? minAge,
    int choreDuration,
    int everyX);
