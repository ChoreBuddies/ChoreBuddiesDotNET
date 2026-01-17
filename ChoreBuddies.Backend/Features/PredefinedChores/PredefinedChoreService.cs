using ChoreBuddies.Backend.Domain;

namespace ChoreBuddies.Backend.Features.DefaultChores;

public interface IPredefinedChoreService
{
    public Task<IEnumerable<PredefinedChore>> GetAllPredefinedChoresAsync();
    public Task<IEnumerable<PredefinedChore>> GetPredefinedChoreAsync(List<int> predefinedChoreIds);
}

public class PredefinedChoreService(IPredefinedChoreRepository repository) : IPredefinedChoreService
{
    private IPredefinedChoreRepository _repository = repository;

    public async Task<IEnumerable<PredefinedChore>> GetAllPredefinedChoresAsync()
    {
        return await _repository.GetAllPredefinedChoreAsync();
    }

    public async Task<IEnumerable<PredefinedChore>> GetPredefinedChoreAsync(List<int> predefinedChoreIds)
    {
        return await _repository.GetPredefinedChoreAsync(predefinedChoreIds);
    }
}
