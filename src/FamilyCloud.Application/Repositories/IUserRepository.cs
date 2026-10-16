using FamilyCloud.Domain;

namespace FamilyCloud.Application.Repositories
{
    public interface IUserRepository
    {
        Task<bool> ExistsByPersonIdAsync(Guid personId, CancellationToken cancellationToken);

        Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);

        Task<bool> HasAdministratorAsync(CancellationToken cancellationToken);

        void Add(User person);
    }
}