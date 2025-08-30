using ChoreBuddies.Backend.Features.Chores;

namespace ChoreBuddies.Backend.Chores
{
	public record ChoreDto(
	string Id,
	string Name,
	string Description,
	string? AssignedTo,
	DateTime DueDate,
	Status Status,
	string Room,
	int RewardPointsCount = 0
);
}
