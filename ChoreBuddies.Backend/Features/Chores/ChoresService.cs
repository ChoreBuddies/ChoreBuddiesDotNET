
using AutoMapper;
using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.DefaultChores;
using ChoreBuddies.Backend.Features.Households;
using Shared.Chores;

namespace ChoreBuddies.Backend.Features.Chores;

public class ChoresService : IChoresService
{
    private readonly IChoresRepository _repository;
    private readonly IMapper _mapper;
    readonly IEnumerable<Chore> mockChores = new List<Chore>
   {
        new Chore(
             Guid.NewGuid().ToString(),
             "Clean Kitchen",
             "Wipe counters, sweep floor, and take out trash",
             "Alice",
             DateTime.UtcNow.AddDays(2),
             Status.Completed,
             "Kitchen",
             10
        ),
        new Chore(
             Guid.NewGuid().ToString(),
            "Vacuum Living Room",
             "Vacuum carpet and dust surfaces",
            "Bob",
            DateTime.UtcNow.AddDays(1),
            Status.Assigned,
             "Living Room",
             15
        ),
        new Chore(
             Guid.NewGuid().ToString(),
             "Laundry",
            "Wash, dry, and fold clothes",
             null,
            DateTime.UtcNow.AddDays(3),
            Status.Unassigned,
             "Laundry Room",
            20
        )
    };

    public ChoresService(IMapper mapper, IChoresRepository choresRepository)
    {
        _mapper = mapper;
        _repository = choresRepository;
    }

    public IEnumerable<ChoreOverviewDto> GetChores()
    {
        return mockChores.Select((t) => _mapper.Map<ChoreOverviewDto>(t));
    }

    public ChoreDto GetChoreDetails(string id)
    {
        return _mapper.Map<ChoreDto>(mockChores.First(t => t.Id == id));
    }

    public async Task<Chore?> CreateChoreAsync(CreateChoreDto createChoreDto)
    {
        var newChore = _mapper.Map<Chore>(createChoreDto);
        return await _repository.CreateChoreAsync(newChore);
    }

    public async Task<Chore?> UpdateChoreAsync(string choreId, CreateChoreDto createChoreDto)
    {
        var chore = await _repository.GetChoreByIdAsync(choreId);
        if (chore != null)
        {
            var newChore = _mapper.Map<Chore>(createChoreDto);
            newChore.Id = choreId;
            return await _repository.UpdateChoreAsync(newChore);
        }
        else
        {
            return null;
        }
    }

    public async Task<Chore?> DeleteChoreAsync(string choreId)
    {
        var chore = await _repository.GetChoreByIdAsync(choreId);
        if (chore != null)
        {
            return await _repository.DeleteChoreAsync(chore);
        }
        else
        {
            return null;
        }
    }
}
