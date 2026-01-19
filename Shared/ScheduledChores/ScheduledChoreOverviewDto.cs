namespace Shared.PredefinedChores;

public record ScheduledChoreOverviewDto(
int Id,
string Name,
int? UserId,
string Room
);
