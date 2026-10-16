using FamilyCloud.Application.Repositories;
using FamilyCloud.Domain;
using FamilyCloud.Infrastructure.DataAccess;

namespace FamilyCloud.Infrastructure.Repositories
{
    public class PersonRepository : IPersonRepository
    {
        private readonly FamilyCloudDbContext _dbContext;

        public PersonRepository(FamilyCloudDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Add(Person person)
        {
            _dbContext.Persons.Add(person);
        }

        public async Task<Person?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _dbContext.Persons.FindAsync(id, cancellationToken);
        }
    }
}