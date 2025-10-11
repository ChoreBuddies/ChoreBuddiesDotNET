namespace ChoreBuddies.Backend.Domain;

// TODO add [StringLength(50, MinimumLength = 1)]
public class DefaultChore
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Frequency { get; set; }
    public int MinAge { get; set; }
    public int ChoreDuration { get; set; }
    public int RewardPointsCount { get; set; }
    public required string Room { get; set; }
}
