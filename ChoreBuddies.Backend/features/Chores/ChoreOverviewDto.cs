using ChoreBuddies.Backend.features.Chores;

namespace ChoreBuddies.Backend.Chores
{
	public record ChoreOverviewDto(
	string Id,
	string Name,
	string? AssignedTo,
	Status Status,
	string Room
);
}
