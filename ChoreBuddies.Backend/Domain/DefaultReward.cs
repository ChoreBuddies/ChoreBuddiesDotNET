namespace ChoreBuddies.Backend.Domain;

public class DefaultReward
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public int SuggestedPoints { get; set; }

}
