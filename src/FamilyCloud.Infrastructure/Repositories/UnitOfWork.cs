using FamilyCloud.Application.Repositories;
using FamilyCloud.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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

        public async Task AcquireBootstrapLockAsync(CancellationToken cancellationToken)
        {
            var connection = _context.Database.GetDbConnection();
            await using var command = connection.CreateCommand();

            command.CommandText = "SELECT pg_advisory_xact_lock(12345)";

            command.Transaction = _context.Database.CurrentTransaction!.GetDbTransaction();

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
