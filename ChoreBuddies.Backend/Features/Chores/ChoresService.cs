using AutoMapper;
using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Notifications;
using ChoreBuddies.Backend.Features.Users;
using Shared.Chores;

namespace ChoreBuddies.Backend.Features.Chores;

public class ChoresService : IChoresService
{
    private readonly IChoresRepository _repository;
    private readonly IAppUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationsService;
    public ChoresService(IMapper mapper, IChoresRepository choresRepository, IAppUserRepository appUserRepository, INotificationService notificationService)
    {
        _mapper = mapper;
        _repository = choresRepository;
        _userRepository = appUserRepository;
        _notificationsService = notificationService;
    }

    public async Task<ChoreDto?> GetChoreDetailsAsync(int choreId)
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

    public async Task<ChoreDto?> DeleteChoreAsync(int choreId)
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

    public async Task<IEnumerable<ChoreDto>> GetUsersChoreDetailsAsync(int userId)
    {
        return _mapper.Map<List<ChoreDto>>(await _repository.GetUsersChoresAsync(userId));
    }

    public async Task<IEnumerable<ChoreDto>> GetMyHouseholdChoreDetailsAsync(int userId)
    {
        return _mapper.Map<List<ChoreDto>>(await _repository.GetHouseholdChoresAsync(userId));
    }

    public async Task<IEnumerable<ChoreDto>> CreateChoreListAsync(IEnumerable<CreateChoreDto> createChoreDtoList)
    {
        return _mapper.Map<List<ChoreDto>>(await _repository.CreateChoreListAsync(_mapper.Map<List<Chore>>(createChoreDtoList)));
    }

    public async Task AssignChoreAsync(ChoreDto choreDto, int userId)
    {
        var assignee = await _userRepository.GetUserByIdAsync(userId);
        if (assignee != null)
        {
            await _repository.AssignChoreAsync(userId, _mapper.Map<Chore>(choreDto));
            await _notificationsService.SendNewChoreNotificationAsync(assignee, choreDto.Name, choreDto.Description, choreDto.DueDate);
        }
    }

    public async Task<ChoreDto> MarkChoreAsDone(int choreId, int userId)
    {
        var chore = await _repository.GetChoreByIdAsync(choreId) ?? throw new Exception("Chore not found");
        if (chore.Status != Status.Assigned)
            throw new Exception("Chore must be assigned in order to be able to mark as done");
        if (chore.UserId != userId)
            throw new Exception("Only user who has to do this chore can mark it as done");
        chore.Status = Status.Completed;
        var user = await _userRepository.GetUserByIdAsync(userId) ?? throw new Exception("User not found");
        user.PointsCount += chore!.RewardPointsCount;
        return _mapper.Map<ChoreDto>(await _repository.UpdateChoreAsync(chore!));
    }
}
