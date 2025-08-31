using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Database;

namespace ChoreBuddies.Backend.Features.DefaultChores
{
    public interface IDefaultChoreRepository
    {
        public Task<ICollection<DefaultChore>> GetAllDefaultChores();
    }

    public class DefaultChoreRepository(ChoreBuddiesDbContext dbContext) : IDefaultChoreRepository
    {
        private ChoreBuddiesDbContext _dbContext = dbContext;

        public Task<ICollection<DefaultChore>> GetAllDefaultChores()
        {
            throw new NotImplementedException();
        }
    }
}
