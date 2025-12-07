namespace Shared.Rewards;
public record RewardDto(int Id,
    string Name,
    string Description,
    int HouseholdId,
    int QuantityAvailable);
