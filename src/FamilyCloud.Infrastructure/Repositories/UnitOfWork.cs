using FamilyCloud.Application.Repositories;
using FamilyCloud.Infrastructure.DataAccess;

namespace FamilyCloud.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FamilyCloudDbContext _context;

        public UnitOfWork(FamilyCloudDbContext context)
        {
            _context = context;
        }

        public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken)
        {
            var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            return new Transaction(transaction);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
