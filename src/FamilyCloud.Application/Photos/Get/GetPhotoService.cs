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

        public async Task<GetPhotoResult?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            Photo? photo = await _photoRepository.GetByIdAsync(id, cancellationToken);
            if (photo == null)
                return null;

            Stream stream = _fileStorage.OpenRead(photo.StorageKey);

            return new GetPhotoResult(stream, photo.MimeType, photo.SourceName);
        }
    }
}
