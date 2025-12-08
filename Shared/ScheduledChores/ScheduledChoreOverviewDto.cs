namespace Shared.ScheduledChores;

public record ScheduledChoreOverviewDto(
int Id,
string Name,
int? UserId,
string Room
);
