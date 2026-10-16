using FamilyCloud.Application.Exceptions;
using FamilyCloud.Application.Photos.CreateThumbnail;
using FamilyCloud.Application.Photos.Events;
using FamilyCloud.Infrastructure.RabbitMq;
using FamilyCloud.Infrastructure.RabbitMq.Publishers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Text;
using System.Text.Json;

namespace FamilyCloud.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly RabbitMqConnection _rabbitMqConnection;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public Worker(
        RabbitMqConnection rabbitMqConnection,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<Worker> logger)
    {
        _rabbitMqConnection = rabbitMqConnection;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        IConnection connection = null!;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Попытка соединения с RabbitMQ");
                connection = await _rabbitMqConnection.GetConnectionAsync(stoppingToken);
                _logger.LogInformation("Соединение с RabbitMQ успешно");

                if (connection.IsOpen)
                {
                    _logger.LogInformation("Попытка создания канала RabbitMQ");
                    await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
                    _logger.LogInformation("Канал создан в RabbitMQ");

                    var consumer = new AsyncEventingBasicConsumer(channel);
                    consumer.ReceivedAsync
                        += async (sender, @events) => await ConsumerReceivedAsync(channel, @events, stoppingToken);

                    await channel.BasicQosAsync(
                        prefetchSize: 0,
                        prefetchCount: 1,
                        global: false,
                        cancellationToken: stoppingToken
                        );

                    await channel.BasicConsumeAsync(
                        queue: RabbitMqTopology.ThumbnailQueue,
                        autoAck: false,
                        consumer: consumer);

                    await Task.Delay(Timeout.Infinite, stoppingToken);
                }
                await Task.Delay(5000, stoppingToken);
            }
            catch (BrokerUnreachableException exp)
            {
                _logger.LogWarning(exp, "Не удалось подключиться к RabbitMq");

                await Task.Delay(5000, stoppingToken);
            }
            catch (AlreadyClosedException exp)
            {
                _logger.LogWarning(exp, "Соединение или канал RabbitMQ закрыт");

                await Task.Delay(5000, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, "Не предвиденная ошибка");

                throw;
            }
        }
    }

    private async Task ConsumerReceivedAsync(
        IChannel channel,
        BasicDeliverEventArgs @event,
        CancellationToken cancellationToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();

        try
        {
            var json = Encoding.UTF8.GetString(@event.Body.ToArray());

            var photoUploaded = JsonSerializer.Deserialize<PhotoUploaded>(json);
            if (photoUploaded == null)
            {
                throw new InvalidOperationException("Не удалось получить Id фотографии");
            }


            CreateThumbnailService createThumbnailService = scope.ServiceProvider.GetRequiredService<CreateThumbnailService>();

            await createThumbnailService.CreateAsync(photoUploaded.PhotoId, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (InvalidPhotoException exp)
        {
            _logger.LogError(exp, "Ошибка обработки фотографии");

            await channel.BasicRejectAsync(@event.DeliveryTag, false, cancellationToken);

            return;
        }
        catch (Exception exp)
        {
            _logger.LogError(exp, "Ошибка обработки фотографии");

            int count = 0;
            if (@event.BasicProperties.Headers != null)
            {
                if (@event.BasicProperties.Headers.TryGetValue("x-retry-count", out object? value))
                {
                    if (value is int retryCount)
                    {
                        count = retryCount;
                    }
                }
            }

            if (count >= 3)
            {
                await channel.BasicRejectAsync(@event.DeliveryTag, false, cancellationToken);

                return;
            }

            var retryPublisher = scope.ServiceProvider.GetRequiredService<RabbitMqRetryPublisher>();
            await retryPublisher.PublishAsync(@event.Body, @event.BasicProperties.Headers, count, cancellationToken);
            await channel.BasicAckAsync(@event.DeliveryTag, false, cancellationToken);

            return;
        }

        await channel.BasicAckAsync(@event.DeliveryTag, false, cancellationToken);
    }
}