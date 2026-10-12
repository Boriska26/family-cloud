using FamilyCloud.Application.Photos.Events;
using FamilyCloud.Application.Repositories;
using FamilyCloud.Application.Services.FileStorage;
using FamilyCloud.Domain;

namespace FamilyCloud.Application.Photos.Upload
{
    public class UploadPhotoService
    {
        private readonly IFileStorage _fileStorage;
        private readonly IPhotoRepository _photoRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventPublisher _eventPublisher;

        public UploadPhotoService(
            IFileStorage fileStorage,
            IPhotoRepository photoRepository,
            IUnitOfWork unitOfWork,
            IEventPublisher eventPublisher)
        {
            _fileStorage = fileStorage;
            _photoRepository = photoRepository;
            _unitOfWork = unitOfWork;
            _eventPublisher = eventPublisher;
        }

        public async Task<Guid> UploadAsync(
            UploadPhotoRequest request,
            CancellationToken cancellationToken)
        {
            Photo photo = Photo.Create(request.SourceName, request.Size, request.MimeType);
            await using var transaction = await _unitOfWork
                .BeginTransactionAsync(cancellationToken);

            string extension = Path.GetExtension(request.SourceName);

            FileSaveOptions fileSaveOptions = new FileSaveOptions(photo.Id, extension, PhotoVariant.Original);
            string storageKey = await _fileStorage.SaveFileAsync(
                request.Stream,
                fileSaveOptions,
                cancellationToken);

            PhotoFile photoFile = photo.GetOriginalFile();
            photoFile.SetStorageKey(storageKey);
            photo.MarkAsUploadedAt(DateTimeOffset.UtcNow);

            _photoRepository.Add(photo);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            await _eventPublisher.PublishAsync(new PhotoUploaded(photo.Id), cancellationToken);

            return photo.Id;
        }
    }
}