namespace FamilyCloud.Application.Repositories
{
    public interface IUnitOfWork
    {
        Task SaveChangesAsync(CancellationToken cancellationToken);

        Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken);
    }
}
