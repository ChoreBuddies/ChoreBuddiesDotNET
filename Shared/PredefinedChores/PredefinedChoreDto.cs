namespace Shared.DefalutChores;

public record PredefinedChoreDto(
    string name,
    string description,
    DateTime dueDate,
    string room,
    int rewardPointsCount
    );
