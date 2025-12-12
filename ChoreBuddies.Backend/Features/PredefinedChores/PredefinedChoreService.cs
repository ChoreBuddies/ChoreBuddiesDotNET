using ChoreBuddies.Backend.Domain;

namespace ChoreBuddies.Backend.Features.DefaultChores;

public interface IPredefinedChoreService
{
    public Task<ICollection<PredefinedChore>> GetAllPredefinedChoresAsync();
}

public class PredefinedChoreService(IPredefinedChoreRepository repository) : IPredefinedChoreService
{
    private IPredefinedChoreRepository _repository = repository;

    public async Task<ICollection<PredefinedChore>> GetAllPredefinedChoresAsync()
    {
        return await _repository.GetAllPredefinedChoreAsync();
    }
}
