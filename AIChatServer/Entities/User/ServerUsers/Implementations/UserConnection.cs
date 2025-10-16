using AIChatServer.Entities.Connection.Interfaces;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace AIChatServer.Entities.User.ServerUsers.Implementations
{
    public class UserConnection(ILogger<UserConnection> logger) : IUserConnection
    {
        private readonly ConcurrentDictionary<Guid, IConnection> _connections = new();
        private readonly ILogger<UserConnection> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public void AddConnection(IConnection connection)
        {
            ArgumentNullException.ThrowIfNull(connection);

            if (_connections.TryAdd(connection.Id, connection))
            {
                _logger.LogInformation("Connection {ConnectionId} added.", connection.Id);
            }
            else
            {
                _logger.LogWarning("Failed to add connection {ConnectionId}, already exists.", connection.Id);
            }
        }

        public IConnection? RemoveConnection(Guid id)
        {
            if (_connections.TryRemove(id, out var connection))
            {
                _logger.LogInformation("Connection {ConnectionId} removed.", id);
                return connection;
            }

            _logger.LogWarning("Attempted to remove non-existing connection {ConnectionId}.", id);
            return null;
        }

        public IReadOnlyCollection<IConnection> GetConnections()
        {
            return _connections.Values.ToList().AsReadOnly();
        }

        public IConnection? GetConnection(Guid id)
        {
            return _connections.TryGetValue(id, out var connection) ? connection : null;
        }
    }
}
