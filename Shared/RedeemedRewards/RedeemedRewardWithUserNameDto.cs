namespace Shared.RedeemedRewards;
public record RedeemedRewardWithUserNameDto(int Id,
    int UserId,
    string UserName,
    string Name,
    int PointsSpent);
