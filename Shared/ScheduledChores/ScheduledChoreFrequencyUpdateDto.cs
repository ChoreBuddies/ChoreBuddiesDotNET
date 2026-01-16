namespace Shared.ScheduledChores;

public record ScheduledChoreFrequencyUpdateDto
{
    public int Id { get; init; }
    public Frequency Frequency { get; init; }
}
