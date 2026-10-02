using FamilyCloud.Application.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace FamilyCloud.Infrastructure.Repositories
{
    public class Transaction : ITransaction
    {
        private readonly IDbContextTransaction _transaction;

        public Transaction(IDbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        public async Task CommitAsync(CancellationToken cancellationToken)
        {
            await _transaction.CommitAsync(cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            await _transaction.DisposeAsync();
        }
    }
}
