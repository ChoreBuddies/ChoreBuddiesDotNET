using Shared.PredefinedChores;

namespace ChoreBuddies.Backend.Domain;

public class PredefinedChore
    (
    string name,
    string description,
    string room,
    int rewardPointsCount,
    int choreDuration,
    Frequency frequency,
    int everyX
    )
{
    public PredefinedChore() : this("", "", "", -1, -1, Frequency.Daily, -1) { }

    public int Id { get; set; }
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public string Room { get; set; } = room;
    public int RewardPointsCount { get; set; } = rewardPointsCount;
    public int ChoreDuration { get; set; } = choreDuration; // Days
    public Frequency Frequency { get; set; } = frequency;
    public int EveryX { get; set; } = everyX;
}

