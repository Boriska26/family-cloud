namespace FamilyCloud.Infrastructure.DataAccess.Outbox
{
    public class OutboxMessage
    {
        private OutboxMessage()
        {

        }

        public OutboxMessage(string eventType, string payload)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
            ArgumentException.ThrowIfNullOrWhiteSpace(payload);

            Id = Guid.NewGuid();
            EventType = eventType;
            Payload = payload;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public Guid Id { get; private set; }

        public string EventType { get; private set; } = string.Empty;

        public string Payload { get; private set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset? PublishedAt { get; private set; }

        public void MarkAsPublished(DateTimeOffset publishAt)
        {
            if (PublishedAt == null)
            {
                PublishedAt = publishAt;
            }
        }
    }
}