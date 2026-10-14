using System.Threading.Channels;
using RabbitMQ.Client;

namespace FamilyCloud.Infrastructure.RabbitMq.Publishers
{
    public class RabbitMqRetryPublisher
    {
        private RabbitMqConnection _rabbitMqConnection;

        public RabbitMqRetryPublisher(RabbitMqConnection rabbitMqConnection)
        {
            _rabbitMqConnection = rabbitMqConnection;
        }

        public async Task PublishAsync(
            ReadOnlyMemory<byte> body,
            IDictionary<string, object?>? headers,
            int retryCount,
            CancellationToken cancellationToken)
        {
            IConnection connection = await _rabbitMqConnection.GetConnectionAsync(cancellationToken);

            CreateChannelOptions createChannelOptions = new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true
            );

            await using IChannel channel = await connection.CreateChannelAsync(
                options: createChannelOptions,
                cancellationToken: cancellationToken);

            IDictionary<string, object?> publishHeaders = headers is null ?
                new Dictionary<string, object?>() :
                new Dictionary<string, object?>(headers);

            publishHeaders["x-retry-count"] = retryCount + 1;

            BasicProperties basicProperties = new BasicProperties
            {
                ContentType = "application/json",
                Persistent = true,
                Headers = publishHeaders
            };

            await channel.BasicPublishAsync(
                exchange: RabbitMqTopology.PhotoRetryExchange,
                routingKey: RabbitMqTopology.ThumbnailRetryRoutingKey,
                body: body,
                mandatory: true,
                basicProperties: basicProperties,
                cancellationToken: cancellationToken
                );
        }
    }
}