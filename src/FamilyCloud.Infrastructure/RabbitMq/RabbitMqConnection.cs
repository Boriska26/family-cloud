using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FamilyCloud.Infrastructure.RabbitMq
{
    public sealed class RabbitMqConnection : IAsyncDisposable
    {
        private readonly ILogger<RabbitMqConnection> _logger;
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly RabbitMqOptions _options;
        private IConnection? _connection;

        public RabbitMqConnection(IOptions<RabbitMqOptions> options, ILogger<RabbitMqConnection> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken)
        {
            if (_connection != null)
            {
                return _connection;
            }
            await _semaphoreSlim.WaitAsync(cancellationToken);
            try
            {
                if (_connection != null)
                {
                    return _connection;
                }
                var factory = new ConnectionFactory();
                factory.UserName = _options.UserName;
                factory.Password = _options.Password;
                factory.Port = _options.Port;
                factory.HostName = _options.Host;
                factory.AutomaticRecoveryEnabled = true;
                factory.TopologyRecoveryEnabled = true;

                _connection = await factory.CreateConnectionAsync(cancellationToken);

                _connection.ConnectionShutdownAsync 
                    += async (object obj,ShutdownEventArgs eventArgs) => await OnConnectionShutdownAsync(eventArgs);

                _connection.RecoverySucceededAsync 
                    += async (object obj, AsyncEventArgs args) => await OnRecoverySucceededAsync(args);
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return _connection;
        }

        private Task OnConnectionShutdownAsync(ShutdownEventArgs eventArgs)
        {
            _logger.LogInformation($"Соединение закрыто", eventArgs.ReplyText);

            return Task.CompletedTask;
        }

        private Task OnRecoverySucceededAsync(AsyncEventArgs args)
        {
            _logger.LogInformation("Соединение успешно востановлено");

            return Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection != null)
            {
                await _connection.DisposeAsync();
            }

            _semaphoreSlim.Dispose();
        }
    }
}