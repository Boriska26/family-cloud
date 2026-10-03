using FamilyCloud.Application.Common;
using FamilyCloud.Application.Repositories;
using FamilyCloud.Application.Services.FileStorage;
using FamilyCloud.Domain;

namespace FamilyCloud.Application.Photos.Get
{
    public class GetPhotoService
    {
        private readonly IPhotoRepository _photoRepository;
        private readonly IFileStorage _fileStorage;

        public GetPhotoService(IPhotoRepository photoRepository, IFileStorage fileStorage)
        {
            _photoRepository = photoRepository;
            _fileStorage = fileStorage;
        }

        public async Task<GetPhotoResult?> GetFileAsync(Guid id, PhotoVariant variant, CancellationToken cancellationToken)
        {
            Photo? photo = await _photoRepository.GetByIdAsync(id, cancellationToken);
            if (photo == null)
                return null;

            PhotoFile? photoFile = variant switch
            {
                PhotoVariant.Original => photo.GetOriginalFile(),
                _ => photo.GetFile(variant)
            };
            if(photoFile  == null)
            {
                return null;
            }
            Stream stream = _fileStorage.OpenRead(photoFile.StorageKey);

            return new GetPhotoResult(stream, photoFile.MimeType, photo.SourceName);
        }

        public async Task<PageResult<PhotoListItem>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken)
        {
            if (page < 1) page = 1;

            if (pageSize <= 0) pageSize = 1;
            else if (pageSize > 100) pageSize = 100;

            var photosPage = await _photoRepository.GetPageAsync(page, pageSize, cancellationToken);
            PageResult<PhotoListItem> pageResult = new PageResult<PhotoListItem>(
                photosPage.Items.Select(n => new PhotoListItem(n.Id, n.SourceName, n.UploadedAt)).ToList(),
                    photosPage.TotalCount, page, pageSize);

            return pageResult;
        }
    }
}
