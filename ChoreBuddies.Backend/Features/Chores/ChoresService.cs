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
    public ChoresService(IMapper mapper, IChoresRepository choresRepository)
    {
        _mapper = mapper;
        _repository = choresRepository;
    }

    public async Task<ChoreDto?> GetChoreDetailsAsync(string choreId)
    {
        var chore = await _repository.GetChoreByIdAsync(choreId);
        if (chore is null) throw new Exception("Chore not found");
        return _mapper.Map<ChoreDto?>(chore);
    }

    public async Task<ChoreDto?> CreateChoreAsync(CreateChoreDto createChoreDto)
    {
        var newChore = _mapper.Map<Chore>(createChoreDto);
        var chore = await _repository.CreateChoreAsync(newChore);
        return _mapper.Map<ChoreDto>(chore);
    }

    public async Task<ChoreDto?> UpdateChoreAsync(ChoreDto choreDto)
    {
        var chore = await _repository.GetChoreByIdAsync(choreDto.Id);
        if (chore != null)
        {
            var newChore = _mapper.Map<Chore>(choreDto);
            var resultChore = await _repository.UpdateChoreAsync(newChore);
            return _mapper.Map<ChoreDto>(resultChore);
        }
        else
        {
            throw new Exception("Chore not found");
        }
    }

    public async Task<ChoreDto?> DeleteChoreAsync(string choreId)
    {
        var chore = await _repository.GetChoreByIdAsync(choreId);
        if (chore != null)
        {
            var resultChore = await _repository.DeleteChoreAsync(chore);
            return _mapper.Map<ChoreDto>(resultChore);
        }
        else
        {
            throw new Exception("Chore not found");
        }
    }
}
