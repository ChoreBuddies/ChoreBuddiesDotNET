namespace Shared.Chores;

public record CreateChoreDto(
    string Name,
    string Description,
    int? UserId,
    int HouseholdId,
    DateTime? DueDate,
    Status? Status,
    string Room,
    int RewardPointsCount);
