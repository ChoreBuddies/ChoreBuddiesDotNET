namespace Shared.Chores;

public record ChoreDto(
int Id,
string Name,
string Description,
string? AssignedTo,
DateTime? DueDate,
Status? Status,
string Room,
int RewardPointsCount = 0
);
