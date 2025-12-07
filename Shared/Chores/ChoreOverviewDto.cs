namespace Shared.Chores;

public record ChoreOverviewDto(
int Id,
string Name,
int? UserId,
Status Status,
string Room
);
