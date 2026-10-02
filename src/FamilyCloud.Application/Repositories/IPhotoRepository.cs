using FamilyCloud.Domain;

namespace FamilyCloud.Application.Repositories
{
    public interface IPhotoRepository
    {
        void Add(Photo photo);

        Task<Photo?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    }
}
