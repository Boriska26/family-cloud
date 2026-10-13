using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FamilyCloud.Infrastructure.RabbitMq
{
    public class RabbitMqTopologyInitializer
    {
        private readonly RabbitMqConnection _rabbitMqConnection;
        private readonly RabbitMqOptions _rabbitMqOptions;

        public RabbitMqTopologyInitializer(
            RabbitMqConnection rabbitMqConnection,
            IOptions<RabbitMqOptions> options)
        {
            _rabbitMqConnection = rabbitMqConnection;
            _rabbitMqOptions = options.Value;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            IConnection connection = await _rabbitMqConnection.GetConnectionAsync(cancellationToken);
            await using IChannel channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await CreatePhotoEventAsync(channel, cancellationToken);
            await CreatePhotoDeadAsync(channel, cancellationToken);
            await CreateReturnsAsync(channel, cancellationToken);
            await CreateRetryAsync(channel, cancellationToken);
        }

        private async Task CreatePhotoEventAsync(IChannel channel, CancellationToken cancellationToken)
        {
            Dictionary<string, object?> arguments = new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = RabbitMqTopology.PhotoDeadExchange,
                ["x-dead-letter-routing-key"] = RabbitMqTopology.ThumbnailFailedRoutingKey,
            };

            await channel.ExchangeDeclareAsync(
                exchange: RabbitMqTopology.PhotoEventsExchange, 
                type: ExchangeType.Fanout,
                durable: true,
                cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(
                queue: RabbitMqTopology.ThumbnailQueue,
                durable:true,
                exclusive: false,
                autoDelete: false,
                arguments: arguments,
                cancellationToken: cancellationToken);

            await channel.QueueBindAsync(
                queue: RabbitMqTopology.ThumbnailQueue,
                exchange: RabbitMqTopology.PhotoEventsExchange,
                routingKey: string.Empty,
                cancellationToken:cancellationToken);
        }

        private async Task CreatePhotoDeadAsync(IChannel channel, CancellationToken cancellationToken)
        {
            await channel.ExchangeDeclareAsync(
                exchange: RabbitMqTopology.PhotoDeadExchange,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken
            );

            await channel.QueueDeclareAsync(
                queue: RabbitMqTopology.ThumbnailFailedQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken
            );

            await channel.QueueBindAsync(
                queue: RabbitMqTopology.ThumbnailFailedQueue,
                exchange: RabbitMqTopology.PhotoDeadExchange,
                routingKey: RabbitMqTopology.ThumbnailFailedRoutingKey,
                cancellationToken: cancellationToken
            );
        }

        private async Task CreateRetryAsync(IChannel channel, CancellationToken cancellationToken)
        {
            Dictionary<string, object?> arguments = new Dictionary<string, object?>
            {
                ["x-message-ttl"] = 30000,
                ["x-dead-letter-exchange"] = RabbitMqTopology.ThumbnailReturnExchange,
                ["x-dead-letter-routing-key"] = RabbitMqTopology.ThumbnailReturnRoutingKey
            };

            await channel.ExchangeDeclareAsync(
                exchange: RabbitMqTopology.PhotoRetryExchange,
                ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken
            );

            await channel.QueueDeclareAsync(
                queue: RabbitMqTopology.ThumbnailRetryQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: arguments,
                cancellationToken: cancellationToken
            );

            await channel.QueueBindAsync(
                queue: RabbitMqTopology.ThumbnailRetryQueue,
                exchange: RabbitMqTopology.PhotoRetryExchange,
                routingKey: RabbitMqTopology.ThumbnailRetryRoutingKey,
                cancellationToken: cancellationToken
            );
        }

        private async Task CreateReturnsAsync(IChannel channel, CancellationToken cancellationToken)
        {
            await channel.ExchangeDeclareAsync(
                exchange: RabbitMqTopology.ThumbnailReturnExchange,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken
            );

            await channel.QueueBindAsync(
                queue: RabbitMqTopology.ThumbnailQueue,
                exchange: RabbitMqTopology.ThumbnailReturnExchange,
                routingKey: RabbitMqTopology.ThumbnailReturnRoutingKey,
                cancellationToken: cancellationToken
            );
        }
    }
}