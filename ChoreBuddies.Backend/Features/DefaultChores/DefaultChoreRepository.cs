using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Database;

namespace ChoreBuddies.Backend.Features.DefaultChores
{
    public interface IDefaultChoreRepository
    {
        public Task<ICollection<DeafultChore>> GetAllDefaultChores();
    }

    public class DefaultChoreRepository(ChoreBuddiesDbContext dbContext) : IDefaultChoreRepository
    {
        private ChoreBuddiesDbContext _dbContext = dbContext;

        public Task<ICollection<DeafultChore>> GetAllDefaultChores()
        {
            throw new NotImplementedException();
        }
    }
}
