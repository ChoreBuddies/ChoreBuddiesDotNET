using AutoMapper;
using ChoreBuddies.Backend.Domain;
using Shared.ScheduledChores;

namespace ChoreBuddies.Backend.Features.ScheduledChores;

public interface IScheduledChoresService
{
    Task<ScheduledChoreDto?> CreateChoreAsync(CreateScheduledChoreDto createScheduledChoreDto, int householdId);
    Task<ScheduledChoreDto?> GetChoreDetailsAsync(int choreId);
    Task<ScheduledChoreDto?> UpdateChoreAsync(ScheduledChoreDto ScheduledChoreDto);
    Task<ScheduledChoreDto?> DeleteChoreAsync(int choreId);
    Task<IEnumerable<ScheduledChoreDto>> GetUsersChoreDetailsAsync(int userId);
    Task<IEnumerable<ScheduledChoreDto>> GetMyHouseholdChoreDetailsAsync(int userId);
}
public class ScheduledChoresService : IScheduledChoresService
{
    private readonly IScheduledChoresRepository _repository;
    private readonly IMapper _mapper;
    public ScheduledChoresService(IMapper mapper, IScheduledChoresRepository scheduedChoresRepository)
    {
        _mapper = mapper;
        _repository = scheduedChoresRepository;
    }

    public async Task<ScheduledChoreDto?> GetChoreDetailsAsync(int choreId)
    {
        var chore = await _repository.GetChoreByIdAsync(choreId);
        if (chore is null) throw new Exception("Chore not found");
        return _mapper.Map<ScheduledChoreDto?>(chore);
    }

    public async Task<ScheduledChoreDto?> CreateChoreAsync(CreateScheduledChoreDto createScheduledChoreDto, int householdId)
    {
        var newChore = _mapper.Map<ScheduledChore>(createScheduledChoreDto);
        newChore.HouseholdId = householdId;
        var chore = await _repository.CreateChoreAsync(newChore);
        return _mapper.Map<ScheduledChoreDto>(chore);
    }

    public async Task<ScheduledChoreDto?> UpdateChoreAsync(ScheduledChoreDto ScheduledChoreDto)
    {
        var chore = await _repository.GetChoreByIdAsync(ScheduledChoreDto.Id);
        if (chore != null)
        {
            var newChore = _mapper.Map<ScheduledChore>(ScheduledChoreDto);
            var resultChore = await _repository.UpdateChoreAsync(newChore);
            return _mapper.Map<ScheduledChoreDto>(resultChore);
        }
        else
        {
            throw new Exception("Chore not found");
        }
    }

    public async Task<ScheduledChoreDto?> DeleteChoreAsync(int choreId)
    {
        var chore = await _repository.GetChoreByIdAsync(choreId);
        if (chore != null)
        {
            var resultChore = await _repository.DeleteChoreAsync(chore);
            return _mapper.Map<ScheduledChoreDto>(resultChore);
        }
        else
        {
            throw new Exception("Chore not found");
        }
    }

    public async Task<IEnumerable<ScheduledChoreDto>> GetUsersChoreDetailsAsync(int userId)
    {
        return _mapper.Map<List<ScheduledChoreDto>>(await _repository.GetUsersChoresAsync(userId));
    }

    public async Task<IEnumerable<ScheduledChoreDto>> GetMyHouseholdChoreDetailsAsync(int userId)
    {
        return _mapper.Map<List<ScheduledChoreDto>>(await _repository.GetHouseholdChoresAsync(userId));
    }
}
