namespace Shared.Chores;

public record ChoreOverviewDto(
string Id,
string Name,
string? AssignedTo,
Status Status,
string Room
);
