using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.ScheduledChores;

namespace ChoreBuddies.Backend.Features.ScheduledChores;

public interface IScheduledChoresRepository
{
    public Task<ICollection<ScheduledChore>> GetAllChoresAsync();
    public Task<ScheduledChore?> CreateChoreAsync(ScheduledChore chore);
    public Task<ScheduledChore?> GetChoreByIdAsync(int id);

    public Task<ScheduledChore?> UpdateChoreAsync(ScheduledChore chore);
    public Task<ScheduledChore?> DeleteChoreAsync(ScheduledChore chore);
    Task<IEnumerable<ScheduledChore>?> GetUsersChoresAsync(int userId);
    Task<IEnumerable<ScheduledChore>?> GetHouseholdChoresAsync(int userId);
    Task<ScheduledChore?> UpdateChoreFrequencyAsync(int choreId, Frequency frequency);

}

public class ScheduledChoresRepository(ChoreBuddiesDbContext dbContext) : IScheduledChoresRepository
{
    private ChoreBuddiesDbContext _dbContext = dbContext;
    public async Task<ScheduledChore?> CreateChoreAsync(ScheduledChore chore)
    {
        var newChore = await _dbContext.AddAsync(chore);
        await _dbContext.SaveChangesAsync();
        return newChore.Entity;
    }

    public async Task<ScheduledChore?> DeleteChoreAsync(ScheduledChore chore)
    {
        _dbContext.ScheduledChores.Remove(chore);
        await _dbContext.SaveChangesAsync();
        return chore;
    }

    public async Task<ICollection<ScheduledChore>> GetAllChoresAsync()
    {
        return await _dbContext.ScheduledChores.ToListAsync();
    }

    public async Task<ScheduledChore?> GetChoreByIdAsync(int id)
    {
        try
        {
            return await _dbContext.ScheduledChores.FindAsync(id);
        }
        catch
        {
            return null;
        }
    }

    public async Task<IEnumerable<ScheduledChore>?> GetHouseholdChoresAsync(int userId)
    {
        try
        {
            var user = await _dbContext.Users
                .Include(u => u.Household)
                .ThenInclude(h => h!.ScheaduledChores)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Household is null || user?.Household?.ScheaduledChores is null)
                return null;
            return user?.Household?.ScheaduledChores;
        }
        catch (NullReferenceException)
        {
            return null;
        }
    }

    public async Task<IEnumerable<ScheduledChore>?> GetUsersChoresAsync(int userId)
    {
        try
        {
            var user = await _dbContext.Users
                .Include(u => u.Household)
                .ThenInclude(h => h!.ScheaduledChores)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Household is null || user?.Household?.ScheaduledChores is null)
                return null;
            return user?.Household?.ScheaduledChores.Where(c => c.UserId == userId);
        }
        catch (NullReferenceException)
        {
            return null;
        }
    }

    public async Task<ScheduledChore?> UpdateChoreAsync(ScheduledChore chore)
    {
        if (chore == null)
            throw new ArgumentNullException(nameof(chore));

        if (chore.Id <= 0)
            throw new ArgumentException("Invalid Chore Id.", nameof(chore));

        if (string.IsNullOrWhiteSpace(chore.Name))
            throw new ArgumentException("Name cannot be empty.", nameof(chore));

        if (string.IsNullOrWhiteSpace(chore.Description))
            throw new ArgumentException("Description cannot be empty.", nameof(chore));

        if (string.IsNullOrWhiteSpace(chore.Room))
            throw new ArgumentException("Room cannot be empty.", nameof(chore));

        if (chore.MinAge < 0)
            throw new ArgumentException("MinAge cannot be negative.", nameof(chore));

        if (chore.ChoreDuration < 1)
            throw new ArgumentException("ChoreDuration must be at least 1.", nameof(chore));

        if (chore.RewardPointsCount < 0)
            throw new ArgumentException("RewardPointsCount cannot be negative.", nameof(chore));

        var existingChore = await _dbContext.Set<ScheduledChore>()
            .FirstOrDefaultAsync(c => c.Id == chore.Id);

        if (existingChore == null)
            return null;

        existingChore.Name = chore.Name;
        existingChore.Description = chore.Description;
        existingChore.UserId = chore.UserId;
        existingChore.Room = chore.Room;
        existingChore.Frequency = chore.Frequency;
        existingChore.RewardPointsCount = chore.RewardPointsCount;
        existingChore.HouseholdId = chore.HouseholdId;
        existingChore.MinAge = chore.MinAge;
        existingChore.ChoreDuration = chore.ChoreDuration;

        await _dbContext.SaveChangesAsync();

        return existingChore;
    }
    public async Task<ScheduledChore?> UpdateChoreFrequencyAsync(int choreId, Frequency frequency)
    {
        var existingChore = await _dbContext.Set<ScheduledChore>()
            .FirstOrDefaultAsync(c => c.Id == choreId);

        if (existingChore == null)
            return null;

        existingChore.Frequency = frequency;
        await _dbContext.SaveChangesAsync();

        return existingChore;
    }
}

