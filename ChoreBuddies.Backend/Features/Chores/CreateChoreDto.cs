using Shared.Chores;
namespace ChoreBuddies.Backend.Features.Chores;

public record CreateChoreDto(
    string Name,
    string Description,
    string? AssignedTo,
    DateTime? DueDate,
    Status? Status,
    string Room,
    int RewardPointsCount)
{
}
