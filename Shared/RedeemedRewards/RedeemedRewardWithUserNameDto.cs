namespace Shared.RedeemedRewards;

public record RedeemedRewardWithUserNameDto
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public required string UserName { get; init; }
    public required string Name { get; init; }
    public int PointsSpent { get; init; }
}

