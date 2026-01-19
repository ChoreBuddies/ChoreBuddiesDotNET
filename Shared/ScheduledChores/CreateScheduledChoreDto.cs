namespace Shared.ScheduledChores;

public record CreateScheduledChoreDto(
    string Name,
    string Description,
    int? UserId,
    string Room,
    int RewardPointsCount,
    Frequency Frequency,
    int ChoreDuration,
    int EveryX);
