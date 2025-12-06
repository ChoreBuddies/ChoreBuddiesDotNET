namespace Shared.Chores;

public record ChoreOverviewDto(
int Id,
string Name,
string? AssignedTo,
Status Status,
string Room
);
