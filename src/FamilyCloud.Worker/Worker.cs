using FamilyCloud.Application.Photos.CreateThumbnail;
using FamilyCloud.Application.Photos.Events;
using FamilyCloud.Infrastructure.RabbitMq;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly RabbitMqOptions _options;

    public Worker(
        RabbitMqConnection rabbitMqConnection,
        IOptions<RabbitMqOptions> options,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<Worker> logger)
    {
        _rabbitMqConnection = rabbitMqConnection;
        _options = options.Value;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = await _rabbitMqConnection.GetConnectionAsync(stoppingToken);

        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (sender, @events) => await ConsumerReceivedAsync(channel, @events, stoppingToken);

        await channel.BasicQosAsync(
            prefetchSize: 0, 
            prefetchCount: 1,
            global: false,
            cancellationToken: stoppingToken
            );

        await channel.BasicConsumeAsync(_options.ThumbnailQueueName, false, consumer);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ConsumerReceivedAsync(
        IChannel channel,
        BasicDeliverEventArgs @event,
        CancellationToken cancellationToken)
    {
        var json = Encoding.UTF8.GetString(@event.Body.ToArray());

        var photoUploaded = JsonSerializer.Deserialize<PhotoUploaded>(json);
        if(photoUploaded == null)
        {
            throw new InvalidOperationException("Не удалось получить Id фотографии");
        }
        
        await using var scope =  _serviceScopeFactory.CreateAsyncScope();

        CreateThumbnailService createThumbnailService = scope.ServiceProvider.GetRequiredService<CreateThumbnailService>();

        _logger.LogInformation("Начинаем создание миниатюры: {PhotoId}", photoUploaded.PhotoId);

        await createThumbnailService.CreateAsync(photoUploaded.PhotoId, cancellationToken);

        _logger.LogInformation("Миниатюра обработана: {PhotoId}", photoUploaded.PhotoId);
        
        await channel.BasicAckAsync(@event.DeliveryTag, false, cancellationToken);
    }
}