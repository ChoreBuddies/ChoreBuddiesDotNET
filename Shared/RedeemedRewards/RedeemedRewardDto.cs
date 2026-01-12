namespace Shared.RedeemedRewards;

public record RedeemedRewardDto(int Id,
    int UserId,
    string Name,
    string Description,
    int PointsSpent,
    bool IsFulfilled);
