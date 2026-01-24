using AutoMapper;
using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Notifications;
using ChoreBuddies.Backend.Features.Users;
using Shared.Chores;

namespace ChoreBuddies.Backend.Features.Chores;

public interface IChoresService
{
    //Create
    public Task<ChoreDto?> CreateChoreAsync(CreateChoreDto createChoreDto);
    // Read
    public Task<ChoreDto?> GetChoreDetailsAsync(int choreId);
    // Update
    public Task<ChoreDto?> UpdateChoreAsync(ChoreDto choreDto);
    // Delete
    public Task<ChoreDto?> DeleteChoreAsync(int choreId);
    public Task<IEnumerable<ChoreDto>> GetUsersChoreDetailsAsync(int userId);
    public Task<IEnumerable<ChoreDto>> GetMyHouseholdChoreDetailsAsync(int userId);
    public Task<IEnumerable<ChoreDto>> CreateChoreListAsync(IEnumerable<CreateChoreDto> createChoreDtoList);
    public Task AssignChoreAsync(int choreId, int userId);
    public Task<ChoreDto> MarkChoreAsDoneAsync(int choreId, int userId, bool verified);
    public Task<ChoreDto> VerifyChoreAsync(int choreId, int userId);
}

public class ChoresService : IChoresService
{
    private readonly IChoresRepository _repository;
    private readonly IMapper _mapper;
    private readonly IAppUserService _appUserService;
    private readonly INotificationService _notificationsService;
    public ChoresService(IMapper mapper, IChoresRepository choresRepository, IAppUserService appUserService, INotificationService notificationService)
    {
        _mapper = mapper;
        _repository = choresRepository;
        _appUserService = appUserService;
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
        if (chore?.UserId is not null)
        {
            await SendNewChoreAssignedNotification((int)chore.UserId, newChore);
        }
        return _mapper.Map<ChoreDto>(chore);
    }

    public async Task<ChoreDto?> UpdateChoreAsync(ChoreDto choreDto)
    {
        var chore = await _repository.GetChoreByIdAsync(choreDto.Id);
        if (chore != null)
        {
            var newChore = _mapper.Map<Chore>(choreDto);
            var resultChore = await _repository.UpdateChoreAsync(newChore);
            if (resultChore?.UserId is not null && chore.UserId != resultChore.UserId)
            {
                await SendNewChoreAssignedNotification((int)resultChore.UserId, newChore);
            }
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

    public async Task AssignChoreAsync(int choreId, int userId)
    {
        var assignee = await _appUserService.GetUserByIdAsync(userId) ?? throw new Exception("User not found");
        var newChore = await _repository.AssignChoreAsync(choreId, userId) ?? throw new Exception("Chore not found");
        await SendNewChoreAssignedNotification(userId, newChore);
    }

    public async Task<ChoreDto> MarkChoreAsDoneAsync(int choreId, int userId, bool verified)
    {
        var chore = await _repository.GetChoreByIdAsync(choreId) ?? throw new Exception("Chore not found");
        if (chore.Status != Status.Assigned)
            throw new Exception("Chore must be assigned in order to be able to mark as done");
        if (chore.UserId != userId)
            throw new Exception("Only user who has to do this chore can mark it as done");
        if (verified)
        {
            chore.Status = Status.Completed;
            if (!await _appUserService.AddPointsToUser(userId, chore!.RewardPointsCount))
            {
                throw new Exception("There was an error while adding points to the user.");
            }
        }
        else
        {
            chore.Status = Status.UnverifiedCompleted;
        }
        return _mapper.Map<ChoreDto>(await _repository.UpdateChoreAsync(chore!));
    }
    public async Task<ChoreDto> VerifyChoreAsync(int choreId, int userId)
    {
        var chore = await _repository.GetChoreByIdAsync(choreId) ?? throw new Exception("Chore not found");
        if (chore.Status != Status.UnverifiedCompleted)
            throw new Exception("Chore must be assigned in order to be able to mark as done");
        if (chore.UserId == userId)
            throw new Exception("Only another user can verify the chore");
        chore.Status = Status.Completed;
        if (!await _appUserService.AddPointsToUser(userId, chore!.RewardPointsCount))
        {
            throw new Exception("There was an error while adding points to the user.");
        }
        return _mapper.Map<ChoreDto>(await _repository.UpdateChoreAsync(chore!));
    }

    private async Task SendNewChoreAssignedNotification(int userId, Chore chore)
    {
        await _notificationsService.SendNewChoreNotificationAsync(userId, chore.Id, chore.Name, chore.Description, chore.DueDate);
    }
}
