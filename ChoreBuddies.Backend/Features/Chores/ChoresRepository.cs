using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Chores;
using ChoreBuddies.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Chores;

namespace ChoreBuddies.Backend.Features.DefaultChores;

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
    Task<IEnumerable<Chore>?> GetUsersChoresAsync(int userId);
    Task<IEnumerable<Chore>?> GetHouseholdChoresAsync(int userId);
    Task<IEnumerable<Chore>?> CreateChoreListAsync(IEnumerable<Chore> createChoreDtoList);
    Task AssignChoreAsync(int userId, Chore choreDto);
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
        var current = GetChoreByIdAsync(chore.Id).Result;
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
            if (chore.AssignedTo is not null && chore.AssignedTo != "")
            {
                current.AssignedTo = chore.AssignedTo;
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
                .Include(u => u.Household)
                .ThenInclude(h => h!.Chores)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Household is null || user?.Household?.Chores is null)
                return null;
            return user?.Household?.Chores.Where(c => c.AssignedTo == user.FirstName);
        }
        catch (NullReferenceException)
        {
            return null;
        }
    }

    public async Task<IEnumerable<Chore>?> GetHouseholdChoresAsync(int userId)
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

    public async Task AssignChoreAsync(int userId, Chore chore)
    {

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return;
        chore.AssignedTo = user.FirstName;
        await _dbContext.SaveChangesAsync();

    }
}
