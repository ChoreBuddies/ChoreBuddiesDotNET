namespace ChoreBuddies.Backend.Domain;

public class DefaultChore
//    (
//    string name,
//    string description,
//    DateTime? dueDate,
//    string room,
//    int rewardPointsCount)
//{
//    public DefaultChore() : this("", "", null,  "", -1) { }

//    public int Id { get; set; } = 0;
//    public string Name { get; set; } = name;
//    public string Description { get; set; } = description;
//    public DateTime? DueDate { get; set; } = dueDate;
//    public string Room { get; set; } = room;
//    public int RewardPointsCount { get; set; } = rewardPointsCount;
//}

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
