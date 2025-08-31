using ChoreBuddies.Backend.Domain;

namespace ChoreBuddies.Backend.Features.Households
{
    public interface IHouseholdService
    {
        public Task<Household?> GetUsersHouseholdAsync(int householdId);
    }

    public class HouseholdService(IHouseholdRepository repository) : IHouseholdService
    {
        private readonly IHouseholdRepository _repository = repository;

        public async Task<Household?> GetUsersHouseholdAsync(int householdId)
        {
            return await _repository.GetUsersHouseholdAsync(householdId);
        }
    }
}