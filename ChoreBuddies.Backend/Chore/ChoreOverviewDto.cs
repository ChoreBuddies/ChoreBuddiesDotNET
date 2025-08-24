namespace ChoreBuddies.Backend.Tasks
{
	public record ChoreOverviewDto(
	string Id,
	string Name,
	string? AssignedTo,
	Status Status,
	string Room
);
	public static partial class TaskExtensions
	{
		public static ChoreOverviewDto ToOverviewDTO(this Chore task)
		{
			return new ChoreOverviewDto(
				Id: task.Id,
				Name: task.Name,
				AssignedTo: task.AssignedTo,
				Status: task.Status,
				Room: task.Room
			);
		}
	}
}
