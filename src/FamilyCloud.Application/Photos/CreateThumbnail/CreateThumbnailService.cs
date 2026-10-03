using FamilyCloud.Application.Repositories;
using FamilyCloud.Application.Services.FileStorage;
using FamilyCloud.Domain;

namespace FamilyCloud.Application.Photos.CreateThumbnail
{
    public class CreateThumbnailService
    {
        private const string ThumbnailExtension = ".webp";
        private const string ThumbnailMimeType = "image/webp";

        private readonly IImageProcessor _imageProcessor;
        private readonly IFileStorage _fileStorage;
        private readonly IPhotoRepository _photoRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateThumbnailService(
            IImageProcessor imageProcessor,
            IFileStorage fileStorage,
            IPhotoRepository photoRepository,
            IUnitOfWork unitOfWork)
        {
            _imageProcessor = imageProcessor;
            _fileStorage = fileStorage;
            _photoRepository = photoRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task CreateAsync(Guid id, CancellationToken cancellationToken)
        {
            Photo? photo = await _photoRepository.GetForUpdateByIdAsync(id, cancellationToken);
            if (photo == null)
            {
                return;
            }
            PhotoFile? thumbnailFile = photo.GetFile(PhotoVariant.Thumbnail);
            if (thumbnailFile != null)
            {
                return;
            }
            PhotoFile originalFile = photo.GetOriginalFile();

            using Stream original = _fileStorage.OpenRead(originalFile.StorageKey);
            using var thumbnail = _imageProcessor.CreateWebpThumbnail(original, 300, 300, 70);
            string storageKey = await _fileStorage.SaveFileAsync(
                thumbnail,
                new FileSaveOptions(
                    photo.Id,
                    ThumbnailExtension,
                    PhotoVariant.Thumbnail),
                cancellationToken);

            long size = thumbnail.Length;
            thumbnailFile = photo.AddFile(PhotoVariant.Thumbnail, size, ThumbnailMimeType);
            thumbnailFile.SetStorageKey(storageKey);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
