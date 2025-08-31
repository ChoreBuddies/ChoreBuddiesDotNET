using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Database;
using Microsoft.EntityFrameworkCore;

namespace ChoreBuddies.Backend.Features.Households
{
    public interface IHouseholdRepository
    {
        public Task<Household?> GetUsersHouseholdAsync(int householdId);
    }

    public class HouseholdRepository(ChoreBuddiesDbContext dbContext) : IHouseholdRepository
    {
        private readonly ChoreBuddiesDbContext _dbContext = dbContext;

        public async Task<Household?> GetUsersHouseholdAsync(int householdId)
        {
            return await _dbContext.Households.FirstOrDefaultAsync(x => x.Id == householdId);
        }
    }
}