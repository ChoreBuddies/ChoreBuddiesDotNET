namespace ChoreBuddies.Backend.Features.DefaultChores
{
    public record DefaultChoreDto(
        string name,
        string description,
        DateTime dueDate,
        string room,
        int rewardPointsCount
        );
}