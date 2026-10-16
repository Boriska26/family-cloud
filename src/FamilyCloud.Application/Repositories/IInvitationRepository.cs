using FamilyCloud.Domain;

namespace FamilyCloud.Application.Repositories
{
    public interface IInvitationRepository
    {
        void Add(Invitation invitation);

        Task<List<Invitation>> GetActiveByPersonIdAsync(
            Guid personId,
            DateTimeOffset now,
            CancellationToken cancellationToken);
    }
}