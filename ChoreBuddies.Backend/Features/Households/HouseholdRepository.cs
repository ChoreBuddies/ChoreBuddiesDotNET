using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Database;
using Microsoft.EntityFrameworkCore;

namespace ChoreBuddies.Backend.Features.Households;

public interface IHouseholdRepository
{

    // Create
    public Task<Household?> CreateHouseholdAsync(Household household);
    // Read
    public Task<Household?> GetHouseholdByIdAsync(int id);
    // Update
    public Task<Household?> UpdateHouseholdAsync(Household household, string? name = null, string? description = null);
    // Delete
    public Task<Household?> DeleteHouseholdAsync(Household household);
}

public class HouseholdRepository(ChoreBuddiesDbContext dbContext) : IHouseholdRepository
{
    private readonly ChoreBuddiesDbContext _dbContext = dbContext;

    public async Task<Household?> CreateHouseholdAsync(Household household)
    {
        var newHousehold = await _dbContext.Households.AddAsync(household);
        await _dbContext.SaveChangesAsync();
        return newHousehold.Entity;
    }
    public async Task<Household?> GetHouseholdByIdAsync(int id)
    {
        try
        {
            return await _dbContext.Households.FindAsync(id);
        }
        catch
        {
            return null;
        }
    }

    public async Task<Household?> UpdateHouseholdAsync(Household household, string? name = null, string? description = null)
    {
        try
        {
            if (name != null && name != "")
            {
                household.Name = name;
            }
            if (description is not null && description != "")
            {
                household.Description = description;
            }
        }
        catch
        {
            return null;
        }
        await _dbContext.SaveChangesAsync();
        return household;
    }

    public async Task<Household?> DeleteHouseholdAsync(Household household)
    {
        _dbContext.Households.Remove(household);
        await _dbContext.SaveChangesAsync();
        return household;
    }

}
