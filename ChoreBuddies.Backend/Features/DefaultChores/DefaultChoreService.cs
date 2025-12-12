using ChoreBuddies.Backend.Domain;

namespace ChoreBuddies.Backend.Features.DefaultChores;

public interface IDefaultChoreService
{
    public Task<ICollection<PredefinedChore>> GetAllDefaultChoresAsync();
}

public class DefaultChoreService(IDefaultChoreRepository repository) : IDefaultChoreService
{
    private IDefaultChoreRepository _repository = repository;

    public async Task<ICollection<PredefinedChore>> GetAllDefaultChoresAsync()
    {
        return await _repository.GetAllDefaultChoreAsync();
    }
}
