using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Chores;

namespace ChoreBuddies.Backend.Features.Chores;

public interface IChoresRepository
{
    public Task<ICollection<Chore>> GetAllChoresAsync();

    //Create
    public Task<Chore?> CreateChoreAsync(Chore chore);

    //Read
    public Task<Chore?> GetChoreByIdAsync(int id);

    //Update
    public Task<Chore?> UpdateChoreAsync(Chore chore);

    //Delete
    public Task<Chore?> DeleteChoreAsync(Chore chore);
    public Task<IEnumerable<Chore>?> GetUsersChoresAsync(int userId);
    public Task<IEnumerable<Chore>?> GetHouseholdChoresAsync(int userId);
    public Task<IEnumerable<Chore>?> GetHouseholdUnverifiedChoresAsync(int userId);
    public Task<IEnumerable<Chore>?> CreateChoreListAsync(IEnumerable<Chore> createChoreDtoList);
    public Task<Chore?> AssignChoreAsync(int choreId, int userId);

}

public class ChoresRepository(ChoreBuddiesDbContext dbContext) : IChoresRepository
{
    private ChoreBuddiesDbContext _dbContext = dbContext;
    public async Task<ICollection<Chore>> GetAllChoresAsync()
    {
        return await _dbContext.Chores.ToListAsync();
    }
    public async Task<Chore?> CreateChoreAsync(Chore chore)
    {
        var newChore = await _dbContext.Chores.AddAsync(chore);
        await _dbContext.SaveChangesAsync();
        return newChore.Entity;
    }

    public async Task<Chore?> GetChoreByIdAsync(int id)
    {
        try
        {
            return await _dbContext.Chores.FindAsync(id);
        }
        catch
        {
            return null;
        }
    }

    public async Task<Chore?> UpdateChoreAsync(Chore chore)
    {
        var current = await GetChoreByIdAsync(chore.Id);
        if (current is null) return current;
        try
        {
            if (chore.Name != null && chore.Name != "")
            {
                current.Name = chore.Name;
            }
            if (chore.Description is not null && chore.Description != "")
            {
                current.Description = chore.Description;
            }
            if (chore.UserId is not null && chore.UserId > 0)
            {
                current.UserId = chore.UserId;
            }
            if (chore.HouseholdId > 0)
            {
                current.HouseholdId = chore.HouseholdId;
            }
            if (chore.DueDate is not null)
            {
                current.DueDate = chore.DueDate;
            }
            if (chore.Status is not null)
            {
                current.Status = chore.Status;
            }
            if (chore.Room is not null && chore.Room != "")
            {
                current.Room = chore.Room;
            }
            if (chore.RewardPointsCount > 0)
            {
                current.RewardPointsCount = chore.RewardPointsCount;
            }
        }
        catch
        {
            return null;
        }
        await _dbContext.SaveChangesAsync();
        return current;
    }

    public async Task<Chore?> DeleteChoreAsync(Chore chore)
    {
        var deletedChore = _dbContext.Chores.Remove(chore);
        await _dbContext.SaveChangesAsync();
        return deletedChore.Entity;
    }

    public async Task<IEnumerable<Chore>?> GetUsersChoresAsync(int userId)
    {
        try
        {
            var user = await _dbContext.Users
                .Include(u => u.Chores)
                .FirstOrDefaultAsync(u => u.Id == userId);
            return user?.Chores ?? [];
        }
        catch (NullReferenceException)
        {
            return null;
        }
    }

    public async Task<IEnumerable<Chore>?> GetHouseholdChoresAsync(int userId) // TODO: change to householdId
    {
        try
        {
            var user = await _dbContext.Users
                .Include(u => u.Household)
                .ThenInclude(h => h!.Chores)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Household is null || user?.Household?.Chores is null)
                return null;
            return user?.Household?.Chores;
        }
        catch (NullReferenceException)
        {
            return null;
        }
    }
    public async Task<IEnumerable<Chore>?> GetHouseholdUnverifiedChoresAsync(int userId)
    {
        try
        {
            var user = await _dbContext.Users
                .Include(u => u.Household)
                .ThenInclude(h => h!.Chores)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Household is null || user?.Household?.Chores is null)
                return null;
            return user?.Household?.Chores?.Where(c => c.Status == Status.UnverifiedCompleted);
        }
        catch (NullReferenceException)
        {
            return null;
        }
    }

    public async Task<IEnumerable<Chore>?> CreateChoreListAsync(IEnumerable<Chore> choreList)
    {
        List<Chore> chores = new List<Chore>();
        foreach (var chore in choreList)
        {
            var newChore = await CreateChoreAsync(chore);
            if (newChore != null)
                chores.Add(chore);
        }
        return chores;
    }

    public async Task<Chore?> AssignChoreAsync(int choreId, int userId)
    {
        var chore = await GetChoreByIdAsync(choreId);
        if (chore is null) return null;
        chore.UserId = userId;
        await _dbContext.SaveChangesAsync();
        return chore;
    }
}
