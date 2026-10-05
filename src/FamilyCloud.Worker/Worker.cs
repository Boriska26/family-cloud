using FamilyCloud.Application.Photos.Events;
using FamilyCloud.Infrastructure.RabbitMq;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace FamilyCloud.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly RabbitMqConnection _rabbitMqConnection;
    private readonly RabbitMqOptions _options;

    public Worker(
        RabbitMqConnection rabbitMqConnection,
        IOptions<RabbitMqOptions> options,
        ILogger<Worker> logger)
    {
        _rabbitMqConnection = rabbitMqConnection;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = await _rabbitMqConnection.GetConnectionAsync(stoppingToken);

        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
        await channel.ExchangeDeclareAsync(_options.ExchangeName, ExchangeType.Fanout, cancellationToken: stoppingToken);

        var queueOk = await channel.QueueDeclareAsync(
            _options.ThumbnailQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            queueOk.QueueName,
            _options.ExchangeName,
            string.Empty,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += Consumer_ReceivedAsync;

        await channel.BasicConsumeAsync(queueOk.QueueName, false, consumer);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task Consumer_ReceivedAsync(object sender, BasicDeliverEventArgs @event)
    {
        var json = Encoding.UTF8.GetString(@event.Body.ToArray());

        var photoUploaded = JsonSerializer.Deserialize<PhotoUploaded>(json);

        _logger.LogInformation("Received PhotoUploaded: {Json}", json);
    }
}