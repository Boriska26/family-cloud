using FamilyCloud.Application.Photos.Events;
using FamilyCloud.Application.Repositories;
using FamilyCloud.Infrastructure.DataAccess.Outbox;
using FamilyCloud.Infrastructure.Repositories;
using RabbitMQ.Client.Exceptions;

namespace FamilyCloud.Worker
{
    public class OutboxDispatcher : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<OutboxDispatcher> _logger;

        public OutboxDispatcher(
            IServiceScopeFactory scopeFactory,
            IEventPublisher eventPublisher,
            ILogger<OutboxDispatcher> logger)
        {
            _scopeFactory = scopeFactory;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await using var scope = _scopeFactory.CreateAsyncScope();
                    OutboxRepository outBoxRepository = scope.ServiceProvider.GetRequiredService<OutboxRepository>();
                    IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var outboxMessages = await outBoxRepository.GetPendingAsync(100, stoppingToken);

                    foreach (OutboxMessage outboxMessage in outboxMessages)
                    {
                        await _eventPublisher.PublishAsync(outboxMessage.Payload, stoppingToken);
                        outboxMessage.MarkAsPublished(DateTimeOffset.UtcNow);
                        await unitOfWork.SaveChangesAsync(stoppingToken);
                    }
                    if (outboxMessages.Count > 0)
                    {
                        _logger.LogInformation(
                            "Обработана пачка Outbox: {Count} сообщений",
                            outboxMessages.Count);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
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
                    _logger.LogError(exp, "Не предвиденая ошибка");

                    throw;
                }
            }
        }
    }
}