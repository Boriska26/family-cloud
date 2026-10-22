using FamilyCloud.Application.Repositories;
using FamilyCloud.Domain;
using FamilyCloud.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace FamilyCloud.Infrastructure.Repositories
{
    public class InvitationRepository : IInvitationRepository
    {
        private readonly FamilyCloudDbContext _dbContext;

        public InvitationRepository(FamilyCloudDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Add(Invitation invitation)
        {
            _dbContext.Invitations.Add(invitation);
        }

        public Task<List<Invitation>> GetActiveByPersonIdAsync(
            Guid personId,
            DateTimeOffset now,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<Invitation?> GetTrackingByTokenHashAsync(string tokenHash, CancellationToken cancellationToken)
        {
            Invitation? invitation = await _dbContext
                .Invitations.FirstOrDefaultAsync(n => n.TokenHash == tokenHash, cancellationToken);

            return invitation;
        }
    }
}