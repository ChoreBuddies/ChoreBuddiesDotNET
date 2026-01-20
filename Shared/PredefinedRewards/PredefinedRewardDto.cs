namespace Shared.PredefinedRewards;

public record PredefinedRewardDto(int Id,
    string Name,
    string Description,
    int Cost,
    int QuantityAvailable);
