using FamilyCloud.Application.Photos.Events;
using RabbitMQ.Client;
using System.Text;

namespace FamilyCloud.Infrastructure.RabbitMq.Publishers
{
    public class RabbitMqEventPublisher : IEventPublisher
    {
        private readonly RabbitMqConnection _rabbitMqConnection;

        public RabbitMqEventPublisher(RabbitMqConnection rabbitMqConnection)
        {
            _rabbitMqConnection = rabbitMqConnection;
        }

        public async Task PublishAsync(string payload, CancellationToken cancellationToken)
        {
            var connection = await _rabbitMqConnection.GetConnectionAsync(cancellationToken);

            CreateChannelOptions createChannelOptions = new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true); 

            await using var channel = await connection.CreateChannelAsync(
                options: createChannelOptions,
                cancellationToken: cancellationToken);

            byte[] photoUploadBytes = Encoding.UTF8.GetBytes(payload);

            BasicProperties basicProperties = new BasicProperties
            {
                ContentType = "application/json",
                Persistent = true
            };

            await channel.BasicPublishAsync(
                exchange: RabbitMqTopology.PhotoEventsExchange,
                routingKey: string.Empty,
                mandatory: true,
                basicProperties: basicProperties,
                body: photoUploadBytes,
                cancellationToken: cancellationToken);
        }
    }
}
