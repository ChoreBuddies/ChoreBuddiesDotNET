using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Households.Exceptions;
using ChoreBuddies.Backend.Features.Users;
using Shared.Households;

namespace ChoreBuddies.Backend.Features.Households;

public interface IHouseholdService
{
    // Create
    public Task<Household?> CreateHouseholdAsync(CreateHouseholdDto createHouseholdDto, int userId);
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
    public async Task<Household?> CreateHouseholdAsync(CreateHouseholdDto createHouseholdDto, int userId)
    {
        var invitationCode = await _invitationCodeService.GenerateUniqueInvitationCodeAsync();
        var result = await _repository.CreateHouseholdAsync(new Household(userId, createHouseholdDto.Name,
            invitationCode, description: createHouseholdDto?.Description));
        await JoinHouseholdAsync(invitationCode, userId);
        return result;

    }

    public async Task<Household?> GetUsersHouseholdAsync(int householdId)
    {
        return await _repository.GetHouseholdByIdAsync(householdId);
    }

    public async Task<Household?> JoinHouseholdAsync(string invitationCode, int userId)
    {
        var user = await _appUserRepository.GetUserByIdAsync(userId);
        if (user is null)
            throw new ArgumentException("User not found");

        var household = await _repository.GetHouseholdByInvitationCodeAsync(invitationCode);
        if (household is null)
            throw new InvalidInvitationCodeException(invitationCode);

        await _repository.JoinHouseholdAsync(household, user);
        return household;
    }

    public async Task<Household?> UpdateHouseholdAsync(int householdId, CreateHouseholdDto createHouseholdDto)
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

    public async Task<Household?> DeleteHouseholdAsync(int householdId)
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

    public async Task<bool> CheckIfUserBelongsAsync(int householdId, int userId)
    {
        return await _repository.CheckIfUserBelongsAsync(householdId, userId);
    }

}
