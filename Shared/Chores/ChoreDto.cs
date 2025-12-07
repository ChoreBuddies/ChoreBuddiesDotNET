namespace Shared.Chores;

public record ChoreDto(
int Id,
string Name,
string Description,
int? UserId,
int HouseholdId,
DateTime? DueDate,
Status? Status,
string Room,
int RewardPointsCount = 0
);
