namespace FamilyCloud.Infrastructure.RabbitMq
{
    public class RabbitMqTopology
    {
        public const string PhotoEventsExchange = "photo.events";
        public const string PhotoDeadExchange = "photo.dead";
        public const string PhotoRetryExchange = "photo.retry";
        public const string ThumbnailReturnExchange = "thumbnail.return";

        public const string ThumbnailQueue = "photo.thumbnail";
        public const string ThumbnailFailedQueue = "photo.thumbnail.failed";
        public const string ThumbnailRetryQueue = "photo.thumbnail.retry";

        public const string ThumbnailFailedRoutingKey = "thumbnail.failed";
        public const string ThumbnailRetryRoutingKey = "thumbnail.retry";
        public const string ThumbnailReturnRoutingKey = "thumbnail.return";
    }
}