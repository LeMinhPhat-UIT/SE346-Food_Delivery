using Messaging.RabbitMq.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Messaging.RabbitMq.Connection
{
    public class ConnectionManager : IAsyncDisposable
    {
        private readonly ConnectionFactory _factory;
        private IConnection? _connection;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly IOptions<RabbitMqOptions> _options;

        public ConnectionManager(IOptions<RabbitMqOptions> options)
        {
            _options = options;

            _factory = new ConnectionFactory
            {
                HostName = options.Value.HostName,
                Port = options.Value.Port,
                UserName = options.Value.UserName,
                Password = options.Value.Password,
                VirtualHost = options.Value.VirtualHost
            };
        }

        public async Task<IConnection> GetConnectionAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (_connection == null || !_connection.IsOpen)
                {
                    _connection = await _factory.CreateConnectionAsync();
                }
                return _connection;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection != null)
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
            }
            _semaphore.Dispose();
        }
    }
}
