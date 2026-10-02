using FamilyCloud.Application.Repositories;
using FamilyCloud.Domain;
using FamilyCloud.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace FamilyCloud.Infrastructure.Repositories
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly FamilyCloudDbContext _context;

        public PhotoRepository(FamilyCloudDbContext context)
        {
            _context = context;
        }

        public void Add(Photo photo)
        {
            _context.Photos.Add(photo);
        }

        public async Task<Photo?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            Photo? photo = await _context.Photos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            return photo;
        }
    }
}