using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChoreBuddies.Backend.Features.DefaultChores;

public interface IChoresRepository
{
    public Task<ICollection<Chore>> GetAllChoresAsync();

    //Create
    public Task<Chore?> CreateChoreAsync(Chore chore);

    //Read
    public Task<Chore?> GetChoreByIdAsync(string id);

    //Update
    public Task<Chore?> UpdateChoreAsync(Chore chore);

    //Delete
    public Task<Chore?> DeleteChoreAsync(Chore chore);
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

    public async Task<Chore?> GetChoreByIdAsync(string id)
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
        _dbContext.Chores.Remove(chore);
        await _dbContext.SaveChangesAsync();
        return chore;
    }

}
