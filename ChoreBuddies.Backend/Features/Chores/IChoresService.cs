using ChoreBuddies.Backend.Domain;
using Shared.Chores;

namespace ChoreBuddies.Backend.Features.Chores;

public interface IChoresService
{
    //Create
    public Task<ChoreDto?> CreateChoreAsync(CreateChoreDto createChoreDto);
    // Read
    public Task<ChoreDto?> GetChoreDetailsAsync(string choreId);
    // Update
    public Task<ChoreDto?> UpdateChoreAsync(ChoreDto choreDto);
    // Delete
    public Task<ChoreDto?> DeleteChoreAsync(string choreId);
    Task<IEnumerable<ChoreDto>> GetUsersChoreDetailsAsync(int userId);
    Task<IEnumerable<ChoreDto>> GetMyHouseholdChoreDetailsAsync(int userId);
    Task<IEnumerable<ChoreDto>> CreateChoreListAsync(IEnumerable<CreateChoreDto> createChoreDtoList);
    Task AssignChoreAsync(ChoreDto choreDto, int userId);
}
