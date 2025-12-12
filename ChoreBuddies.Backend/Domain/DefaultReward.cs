namespace ChoreBuddies.Backend.Domain;

public class DefaultReward(
    string name,
    string description,
    int cost,
    int quantityAvailable
    )
{
    public DefaultReward() : this("", "", 0, 0)
    {
    }

    public int Id { get; set; }
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public int Cost { get; set; } = cost;
    public int QuantityAvailable { get; set; } = quantityAvailable;
}
