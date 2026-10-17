using FamilyCloud.Application.Repositories;
using FamilyCloud.Domain;
using FamilyCloud.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace FamilyCloud.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private FamilyCloudDbContext _dbContext;

        public UserRepository(FamilyCloudDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<bool> ExistsByPersonIdAsync(Guid personId, CancellationToken cancellationToken)
        {
            return _dbContext.Users.AnyAsync(n => n.PersonId == personId, cancellationToken);
        }

        public async Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _dbContext.Users.FindAsync(userId, cancellationToken);
        }

        public Task<bool> HasAdministratorAsync(CancellationToken cancellationToken)
        {
            return _dbContext.Users.AnyAsync(n => n.Role == Role.Administrator, cancellationToken);
        }

        public void Add(User user)
        {
            _dbContext.Users.Add(user);
        }

        public async Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken)
        {
            var lowerLogin = login.ToLowerInvariant();
            return await _dbContext.Users.FirstOrDefaultAsync(n => n.Login == lowerLogin, cancellationToken);
        }
    }
}