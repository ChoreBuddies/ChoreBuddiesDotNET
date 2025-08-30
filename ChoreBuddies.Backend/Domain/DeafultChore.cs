namespace ChoreBuddies.Backend.Domain
{
    public class DeafultChore(
        int id,
        string name,
        string description,
        DateTime dueDate,
        string room,
        int rewardPointsCount)
    {
        public int Id { get; set; } = id;
        public string Name { get; set; } = name;
        public string Description { get; set; } = description;
        public DateTime DueDate { get; set; } = dueDate;
        public string Room { get; set; } = room;
        public int RewardPointsCount { get; set; } = rewardPointsCount;
    }
}