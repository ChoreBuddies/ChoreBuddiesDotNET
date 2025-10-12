using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Households;
using Shared.Chores;

namespace ChoreBuddies.Backend.Features.Chores;

public interface IChoresService
{
    IEnumerable<ChoreOverviewDto> GetChores();

    //Create
    public Task<Chore?> CreateChoreAsync(CreateChoreDto createChoreDto);
    // Read
    ChoreDto GetChoreDetails(string id);
    // Update
    public Task<Chore?> UpdateChoreAsync(string choreId, CreateChoreDto createChoreDto);
    // Delete
    public Task<Chore?> DeleteChoreAsync(string choreId);
}
