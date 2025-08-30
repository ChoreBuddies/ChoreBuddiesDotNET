using ChoreBuddies.Backend.Chores;

namespace ChoreBuddies.Backend.Features.Chores
{
	public interface IChoresService
	{
		IEnumerable<ChoreOverviewDto> GetChores();
		ChoreDto GetChoreDetails(string id);
	}
}
