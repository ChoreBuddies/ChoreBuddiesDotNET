using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Users;

namespace ChoreBuddies.Backend.Features.Households;

public interface IHouseholdService
{
    // Create
    public Task<Household?> CreateHouseholdAsync(CreateHouseholdDto createHouseholdDto);
    // Read
    public Task<Household?> GetUsersHouseholdAsync(int householdId);
    // Update
    public Task<Household?> UpdateHouseholdAsync(int householdId, CreateHouseholdDto createHouseholdDto);
    public Task<Household?> JoinHouseholdAsync(string invitationCode, int userId);
    // Delete
    public Task<Household?> DeleteHouseholdAsync(int householdId);
    // Validate
    public Task<bool> CheckIfUserBelongsAsync(int householdId, int userId);
}

public class HouseholdService(IHouseholdRepository repository, IInvitationCodeService invitationCodeService, IAppUserRepository appUserRepository) : IHouseholdService
{
    private readonly IHouseholdRepository _repository = repository;
    private readonly IAppUserRepository _appUserRepository = appUserRepository;
    private readonly IInvitationCodeService _invitationCodeService = invitationCodeService;
    async Task<Household?> IHouseholdService.CreateHouseholdAsync(CreateHouseholdDto createHouseholdDto)
    {
        var invitationCode = await _invitationCodeService.GenerateUniqueInvitationCodeAsync();
        return await _repository.CreateHouseholdAsync(new Household(Guid.NewGuid().GetHashCode(),
            createHouseholdDto.Name, invitationCode, description: createHouseholdDto?.Description)); //TODO: add OwnerId
    }

    public async Task<Household?> GetUsersHouseholdAsync(int householdId)
    {
        return await _repository.GetHouseholdByIdAsync(householdId);
    }

    public async Task<Household?> JoinHouseholdAsync(string invitationCode, int userId)
    {
        var user = await _appUserRepository.GetUserByIdAsync(userId);
        if (user is null)
            throw new InvalidOperationException($"User not found for ID: {userId}");

        var household = await _repository.GetHouseholdByInvitationCodeAsync(invitationCode);
        if (household is null)
            throw new KeyNotFoundException($"No household found matching invitation code: {invitationCode}");

        await _repository.JoinHouseholdAsync(household, user);
        return household;
    }

    async Task<Household?> IHouseholdService.UpdateHouseholdAsync(int householdId, CreateHouseholdDto createHouseholdDto)
    {
        var household = await _repository.GetHouseholdByIdAsync(householdId);
        if (household != null)
        {
            return await _repository.UpdateHouseholdAsync(household, createHouseholdDto.Name, createHouseholdDto.Description);
        }
        else
        {
            return null;
        }
    }

    async Task<Household?> IHouseholdService.DeleteHouseholdAsync(int householdId)
    {
        var household = await _repository.GetHouseholdByIdAsync(householdId);
        if (household != null)
        {
            return await _repository.DeleteHouseholdAsync(household);
        }
        else
        {
            return null;
        }
    }

    public Task<Household?> GetUsersHouseholdAsync(int householdId, string userId)
    {
        throw new NotImplementedException();
    }

    public Task<Household?> JoinHouseholdAsync(string invitationCode, AppUser user)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> CheckIfUserBelongsAsync(int householdId, int userId)
    {
        return await _repository.CheckIfUserBelongsAsync(householdId, userId);
    }
}
