using ChoreBuddies.Backend.Chores;

namespace ChoreBuddies.Backend.features.Chores
{
	public interface IChoresService
	{
		IEnumerable<ChoreOverviewDto> GetChores();
		ChoreDto GetChoreDetails(string id);
	}
}
