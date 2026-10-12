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

            await channel.ExchangeDeclareAsync(
                exchange: _rabbitMqOptions.ExchangeName, 
                type: ExchangeType.Fanout,
                durable: true,
                cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(
                queue: _rabbitMqOptions.ThumbnailQueueName,
                durable:true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);

            await channel.QueueBindAsync(
                queue: _rabbitMqOptions.ThumbnailQueueName,
                exchange: _rabbitMqOptions.ExchangeName,
                routingKey: string.Empty,
                cancellationToken:cancellationToken);
        }
    }
}