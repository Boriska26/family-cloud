using FamilyCloud.Application.Repositories;
using FamilyCloud.Domain;
using FamilyCloud.Infrastructure.DataAccess;

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
    }
}