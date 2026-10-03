using FamilyCloud.Application.Common;
using FamilyCloud.Domain;

namespace FamilyCloud.Application.Repositories
{
    public interface IPhotoRepository
    {
        void Add(Photo photo);

        Task<Photo?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        Task<PageResult<Photo>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken);

        Task<Photo?> GetForUpdateByIdAsync(Guid id, CancellationToken cancellationToken);
    }
}
