using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FamilyCloud.Infrastructure.RabbitMq
{
    public sealed class RabbitMqConnection : IAsyncDisposable
    {
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly RabbitMqOptions _options;
        private IConnection? _connection;

        public RabbitMqConnection(IOptions<RabbitMqOptions> options)
        {
            _options = options.Value;
        }

        public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken)
        {
            if (_connection != null && _connection.IsOpen)
            {
                return _connection;
            }
            await _semaphoreSlim.WaitAsync(cancellationToken);
            try
            {
                if (_connection != null && _connection.IsOpen)
                {
                    return _connection;
                }
                if (_connection != null && !_connection.IsOpen)
                {
                    await _connection.DisposeAsync();
                    _connection = null;
                }
                var factory = new ConnectionFactory();
                factory.UserName = _options.UserName;
                factory.Password = _options.Password;
                factory.Port = _options.Port;
                factory.HostName = _options.Host;
                factory.AutomaticRecoveryEnabled = true;
                factory.TopologyRecoveryEnabled = true;

                _connection = await factory.CreateConnectionAsync(cancellationToken);
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return _connection;
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
