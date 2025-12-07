namespace Shared.Rewards;
public record CreateRewardDto(string Name,
    string Description,
    int HouseholdId,
    int QuantityAvailable = 1);
