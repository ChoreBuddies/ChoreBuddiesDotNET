namespace Shared.PredefinedChores;

public record ScheduledChoreTileViewDto(int Id, string Name, string? UserName, string Description, Frequency Frequency);
