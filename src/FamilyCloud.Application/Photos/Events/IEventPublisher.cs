namespace FamilyCloud.Application.Photos.Events
{
    public interface IEventPublisher
    {
        Task PublishAsync(string payload, CancellationToken cancellationToken);
    }
}
