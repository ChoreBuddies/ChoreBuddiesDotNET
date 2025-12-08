namespace ChoreBuddies.Backend.Domain;

public class RedeemedReward
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public int UserId { get; set; }
    public virtual AppUser? User { get; set; }
    public int HouseholdId { get; set; }
    public virtual Household? Household { get; set; }
    public DateTime? RedeemedDate { get; set; }
    public int PointsSpent { get; set; }
    public bool IsFulfilled { get; set; } // true jak rodzic zaznaczy, że wydał dziecku
}
