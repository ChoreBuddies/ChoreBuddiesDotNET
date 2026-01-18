using AutoMapper;
using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.PredefinedChores;
using Microsoft.EntityFrameworkCore;
using Shared.ScheduledChores;

namespace ChoreBuddies.Backend.Features.ScheduledChores;

public interface IScheduledChoresService
{
    Task<ScheduledChoreDto?> CreateChoreAsync(CreateScheduledChoreDto createScheduledChoreDto, int householdId);
    Task<ScheduledChoreDto?> GetChoreDetailsAsync(int choreId);
    Task<ScheduledChoreDto?> UpdateChoreAsync(ScheduledChoreDto ScheduledChoreDto);
    Task<ScheduledChoreDto?> UpdateChoreFrequencyAsync(int choreId, Frequency frequency);
    Task<ScheduledChoreDto?> DeleteChoreAsync(int choreId);
    Task<IEnumerable<ScheduledChoreDto>> AddPredefinedChoresToHouseholdAsync(List<int> predefinedChoreIds, int householdId);
    Task<IEnumerable<ScheduledChoreDto>> GetUsersChoreDetailsAsync(int userId);
    Task<IEnumerable<ScheduledChoreDto>> GetMyHouseholdChoresDetailsAsync(int userId);
    Task<IEnumerable<ScheduledChoreTileViewDto>> GetMyHouseholdChoresOverviewDetailsAsync(int userId);

}
public class ScheduledChoresService : IScheduledChoresService
{
    private readonly IScheduledChoresRepository _repository;
    private readonly IPredefinedChoreService _predefinedChoreService;
    private readonly IMapper _mapper;
    public ScheduledChoresService(
        IMapper mapper,
        IScheduledChoresRepository scheduedChoresRepository,
        IPredefinedChoreService predefinedChoreService)
    {
        _mapper = mapper;
        _repository = scheduedChoresRepository;
        _predefinedChoreService = predefinedChoreService;
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

    public async Task<IEnumerable<ScheduledChoreDto>> GetMyHouseholdChoresDetailsAsync(int userId)
    {
        return _mapper.Map<List<ScheduledChoreDto>>(await _repository.GetHouseholdChoresAsync(userId));
    }

    public async Task<ScheduledChoreDto?> UpdateChoreFrequencyAsync(int choreId, Frequency frequency)
    {
        var resultChore = await _repository.UpdateChoreFrequencyAsync(choreId, frequency);
        if (resultChore is null)
        {
            throw new Exception("Updating Chore Frequency Failed");
        }
        return _mapper.Map<ScheduledChoreDto>(resultChore);
    }

    public async Task<IEnumerable<ScheduledChoreDto>> AddPredefinedChoresToHouseholdAsync(List<int> predefinedChoreIds, int householdId)
    {
        var predefinedChores = await _predefinedChoreService.GetPredefinedChoresAsync(predefinedChoreIds);

        var createdChores = new List<ScheduledChore>();
        foreach (var p in predefinedChores)
        {
            var newChore = new ScheduledChore(
                name: p.Name,
                description: p.Description,
                userId: null,
                room: p.Room,
                everyX: p.EveryX,
                frequency: p.Frequency,
                rewardPointsCount: p.RewardPointsCount,
                householdId: householdId,
                choreDuration: p.ChoreDuration
            );

            var createdChore = await _repository.CreateChoreAsync(newChore);
            if (createdChore is null)
                throw new Exception("Creating Chore from Predefined Chore Failed");

            createdChores.Add(createdChore);
        }

        return _mapper.Map<List<ScheduledChoreDto>>(createdChores);
    }

    public async Task<IEnumerable<ScheduledChoreTileViewDto>> GetMyHouseholdChoresOverviewDetailsAsync(int userId)
    {
        return _mapper.Map<List<ScheduledChoreTileViewDto>>(await _repository.GetHouseholdChoresWithUserAsync(userId));
    }
}
