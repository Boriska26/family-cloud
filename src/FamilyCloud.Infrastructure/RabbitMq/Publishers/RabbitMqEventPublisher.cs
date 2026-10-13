using FamilyCloud.Application.Photos.Events;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace FamilyCloud.Infrastructure.RabbitMq.Publishers
{
    public class RabbitMqEventPublisher : IEventPublisher
    {
        private readonly RabbitMqConnection _rabbitMqConnection;

        public RabbitMqEventPublisher(RabbitMqConnection rabbitMqConnection)
        {
            _rabbitMqConnection = rabbitMqConnection;
        }

        public async Task PublishAsync(PhotoUploaded photoUploaded, CancellationToken cancellationToken)
        {
            var connection = await _rabbitMqConnection.GetConnectionAsync(cancellationToken);

            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            string photoUploadJson = JsonSerializer.Serialize(photoUploaded);
            byte[] photoUploadBytes = Encoding.UTF8.GetBytes(photoUploadJson);

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
