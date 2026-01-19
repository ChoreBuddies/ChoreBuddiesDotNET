
namespace Shared.PredefinedChores;

public record PredefinedChoreDto(
    int id,
    string name,
    string description,
    string room,
    int rewardPointsCount,
    int choreDuration,
    Frequency frequency,
    int everyX
    );
