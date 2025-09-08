using ChoreBuddies.Backend.Domain;

namespace ChoreBuddies.Backend.Features.DefaultChores;

public interface IDefaultChoreService
{
    public Task<ICollection<DefaultChore>> GetAllDefaultChoresAsync();
}

public class DefaultChoreService(IDefaultChoreRepository repository) : IDefaultChoreService
{
    private IDefaultChoreRepository _repository = repository;

    public async Task<ICollection<DefaultChore>> GetAllDefaultChoresAsync()
    {
        return await _repository.GetAllDefaultChoreAsync();
    }
}
