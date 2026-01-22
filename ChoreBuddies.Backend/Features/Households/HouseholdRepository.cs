using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChoreBuddies.Backend.Features.Households;

public interface IHouseholdRepository
{

    // Create
    public Task<Household?> CreateHouseholdAsync(Household household);
    // Read
    public Task<Household?> GetHouseholdByIdAsync(int id);
    public Task<Household?> GetHouseholdByInvitationCodeAsync(string invitationCode);
    // Update
    public Task<Household?> UpdateHouseholdAsync(Household household, string? name = null, string? description = null);
    public Task<Household?> JoinHouseholdAsync(Household household, AppUser user);
    // Delete
    public Task<Household?> DeleteHouseholdAsync(Household household);
    public Task<bool> CheckIfUserBelongsAsync(int householdId, int userId);

    public Task<Household?> GeHouseholdWithUsersWithChoresDueToFromDateAsync(int householdId, DateTime fromDate);
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
    public async Task<Household?> GetHouseholdByInvitationCodeAsync(string invitationCode)
    {
        if (string.IsNullOrWhiteSpace(invitationCode))
            return null;

        return await _dbContext.Households
            .FirstOrDefaultAsync(h => h.InvitationCode == invitationCode);
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

    public async Task<Household?> JoinHouseholdAsync(Household household, AppUser user)
    {
        try
        {
            household.Users.Add(user);
        }
        catch
        {
            throw new InvalidOperationException("User already belongs to the household");
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

    public async Task<bool> CheckIfUserBelongsAsync(int householdId, int userId)
    {
        return await _dbContext.Households
            .AnyAsync(h => h.Id == householdId &&
                           h.Users.Any(u => u.Id == userId));
    }

    public async Task<Household?> GeHouseholdWithUsersWithChoresDueToFromDateAsync(int householdId, DateTime fromDate)
    {
        return await _dbContext.Households
                .Include(h => h.Users)
                    .ThenInclude(u => u.Chores
                        .Where(c => c.DueDate >= fromDate))
                .FirstOrDefaultAsync(h => h.Id == householdId);
    }
}
