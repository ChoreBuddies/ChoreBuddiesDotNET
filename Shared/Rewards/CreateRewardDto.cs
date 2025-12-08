namespace Shared.Rewards;
public record CreateRewardDto(string Name,
    string Description,
    int HouseholdId,
    int Cost,
    int QuantityAvailable);
