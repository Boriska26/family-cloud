using FamilyCloud.Application.Common;
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
            Photo? photo = await _context.Photos
                .Include(n => n.Files)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            return photo;
        }

        public async Task<Photo?> GetForUpdateByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            Photo? photo = await _context.Photos
                .Include(n => n.Files)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            return photo;
        }

        public async Task<PageResult<Photo>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken)
        {
            if (pageSize < 1 || pageSize > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize));
            }
            if (page <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(page));
            }

            int skip = (page - 1) * pageSize;

            var items = await _context.Photos.AsNoTracking()
                    .OrderByDescending(n => n.UploadedAt)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);
            var totalCount = await _context.Photos.CountAsync(cancellationToken);

            PageResult<Photo> pageResult = new PageResult<Photo>(items, totalCount, page, pageSize);

            return pageResult;
        }
    }
}