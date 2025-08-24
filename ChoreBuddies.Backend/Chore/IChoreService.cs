using ChoreBuddies.Backend.Tasks;

namespace ChoreBuddies.Backend.Chore
{
	public interface IChoresService
	{
		IEnumerable<ChoreOverviewDto> GetChores();
		ChoreDto GetChoreDetails(string id);
	}
}
