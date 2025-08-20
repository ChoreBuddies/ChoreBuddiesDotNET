namespace ChoreBuddies.Backend.Tasks
{
	public record ChoreDto(
	string Id,
	string Name,
	string Description,
	string? AssignedTo,
	DateTime DueDate,
	Status Status,
	string Room,
	int RewardPointsCount = 0
);
	public static partial class TaskExtensions
	{
		public static ChoreDto ToDTO(this Chore task)
		{
			return new ChoreDto(
				Id: task.Id,
				Name: task.Name,
				Description: task.Description,
				AssignedTo: task.AssignedTo,
				DueDate: task.DueDate,
				Status: task.Status,
				Room: task.Room,
				RewardPointsCount: task.RewardPointsCount
			);
		}
	}
}
