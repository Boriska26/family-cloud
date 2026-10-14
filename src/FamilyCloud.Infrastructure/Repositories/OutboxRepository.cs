using FamilyCloud.Infrastructure.DataAccess;
using FamilyCloud.Infrastructure.DataAccess.Outbox;
using Microsoft.EntityFrameworkCore;

namespace FamilyCloud.Infrastructure.Repositories
{
    public class OutboxRepository
    {
        private readonly FamilyCloudDbContext _dbContext;

        public OutboxRepository(FamilyCloudDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken)
        {
            return await _dbContext.OutboxMessages
                .Where(n=>n.PublishedAt == null)
                .OrderBy(n=>n.CreatedAt)
                .Take(batchSize)
                .ToListAsync(cancellationToken);
        }
    }
}