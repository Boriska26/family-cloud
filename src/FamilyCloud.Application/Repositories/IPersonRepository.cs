using FamilyCloud.Domain;

namespace FamilyCloud.Application.Repositories
{
    public interface IPersonRepository
    {
        Task<Person?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        void Add(Person person);
    }
}