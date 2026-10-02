namespace FamilyCloud.Application.Repositories
{
    public interface ITransaction : IAsyncDisposable
    {
        Task CommitAsync(CancellationToken cancellationToken);
    }
}