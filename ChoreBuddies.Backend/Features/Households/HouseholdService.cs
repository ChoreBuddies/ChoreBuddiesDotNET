using ChoreBuddies.Backend.Domain;

namespace ChoreBuddies.Backend.Features.Households;

public interface IHouseholdService
{
    // Create
    public Task<Household?> CreateHouseholdAsync(CreateHouseholdDto createHouseholdDto);
    // Read
    public Task<Household?> GetUsersHouseholdAsync(int householdId);
    // Update
    public Task<Household?> UpdateHouseholdAsync(int householdId, CreateHouseholdDto createHouseholdDto);
    // Delete
    public Task<Household?> DeleteHouseholdAsync(int householdId);
}

public class HouseholdService(IHouseholdRepository repository) : IHouseholdService
{
    private readonly IHouseholdRepository _repository = repository;
    async Task<Household?> IHouseholdService.CreateHouseholdAsync(CreateHouseholdDto createHouseholdDto)
    {
        return await _repository.CreateHouseholdAsync(new Household(Guid.NewGuid().GetHashCode(),
            createHouseholdDto.Name, description: createHouseholdDto?.Description)); //TODO: add OwnerId
    }

    public async Task<Household?> GetUsersHouseholdAsync(int householdId)
    {
        return await _repository.GetHouseholdByIdAsync(householdId);
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

}
