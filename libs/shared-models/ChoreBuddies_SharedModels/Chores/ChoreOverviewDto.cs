namespace ChoreBuddies_SharedModels.Chores
{
    public record ChoreOverviewDto(
    string Id,
    string Name,
    string? AssignedTo,
    Status Status,
    string Room
);
}