namespace FamilyCloud.Application.Photos.Events
{
    public interface IEventPublisher
    {
        Task PublishAsync(PhotoUploaded photoUploaded, CancellationToken cancellationToken);
    }
}
