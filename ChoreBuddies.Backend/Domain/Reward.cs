namespace ChoreBuddies.Backend.Domain;

public class Reward(
    string name, 
    string description, 
    int householdId,
    int cost,
    int quantityAvailable
    )
{
    public int Id { get; set; } = 0;
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public int HouseholdId { get; set; } = householdId;
    public virtual Household? Household { get; set; }
    public int Cost { get; set; } = cost;
    public int QuantityAvailable { get; set; } = quantityAvailable;
    public virtual ICollection<RedeemedReward>? RedeemedRewards { get; set; } = new HashSet<RedeemedReward>();
}
