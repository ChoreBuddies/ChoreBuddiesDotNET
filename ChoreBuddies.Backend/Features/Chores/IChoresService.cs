using ChoreBuddies.Backend.Domain;
using Shared.Chores;

namespace ChoreBuddies.Backend.Features.Chores;

public interface IChoresService
{
    //Create
    public Task<ChoreDto?> CreateChoreAsync(CreateChoreDto createChoreDto);
    // Read
    public Task<ChoreDto?> GetChoreDetailsAsync(int choreId);
    // Update
    public Task<ChoreDto?> UpdateChoreAsync(ChoreDto choreDto);
    // Delete
    public Task<ChoreDto?> DeleteChoreAsync(int choreId);
    public Task<IEnumerable<ChoreDto>> GetUsersChoreDetailsAsync(int userId);
    public Task<IEnumerable<ChoreDto>> GetMyHouseholdChoreDetailsAsync(int userId);
    public Task<IEnumerable<ChoreDto>> CreateChoreListAsync(IEnumerable<CreateChoreDto> createChoreDtoList);
    public Task AssignChoreAsync(int choreId, int userId);
    public Task<ChoreDto> MarkChoreAsDone(int choreId, int userId);
}
