namespace Shared.ScheduledChores;

public record ScheduledChoreOverviewDto(
int Id,
string Name,
string? AssignedTo,
string Room
);
