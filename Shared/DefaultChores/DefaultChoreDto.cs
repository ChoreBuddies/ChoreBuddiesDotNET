namespace Shared.DefalutChores;

public record DefaultChoreDto(
    string name,
    string description,
    DateTime dueDate,
    string room,
    int rewardPointsCount
    );
