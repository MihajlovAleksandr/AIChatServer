using AIChatServer.Entities.Connection;
using AIChatServer.Entities.Connection.Interfaces;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Service.Interfaces;
using AIChatServer.Utils;
using AIChatServer.Utils.Interfaces;
using AIChatServer.Utils.Interfaces.Mapper;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Entities.User.ServerUsers.Implementations
{
    public class ConnectionNotifier(IConnectionService connectionService, IUserConnection userConnection, ISerializer serializer, IResponseMapper<ConnectionInfoResponse, ConnectionInfo> mapper, ILogger<ConnectionNotifier> logger) : IConnectionNotifier, IDisposable
    {
        private readonly IConnectionService _connectionService = connectionService 
            ?? throw new ArgumentNullException(nameof(connectionService));
        private readonly IUserConnection _userConnection = userConnection 
            ?? throw new ArgumentNullException(nameof(userConnection));
        private readonly ISerializer _serializer = serializer 
            ?? throw new ArgumentNullException(nameof(serializer));
        private readonly IResponseMapper<ConnectionInfoResponse, ConnectionInfo> mapper = mapper
            ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<ConnectionNotifier> _logger = logger 
            ?? throw new ArgumentNullException(nameof(logger));
        private bool _disposed = false;

        public event EventHandler<Command>? GotCommand;
        public event EventHandler? Disconnected;

        public void HandleConnectionAdded(IConnection connection, User user, bool isKnownUser)
        {
            if (_disposed) return;

            connection.CommandGot += OnCommandGot;
            connection.Disconnected += (s, e) => OnConnectionDisconnected(s, user);

            _connectionService.SetLastConnection(connection.Id, true);
            _logger.LogInformation("Connection {ConnectionId} added for user {UserId}. Known user: {IsKnownUser}", connection.Id, user.Id, isKnownUser);

            if (isKnownUser && _userConnection.GetConnections().Count > 1)
            {
                NotifyConnectionsChange(connection, user, true);
            }
        }

        public void HandleConnectionRemoved(IConnection connection, User user)
        {
            if (_disposed) return;
            if (connection == null) return;

            _connectionService.SetLastConnection(connection.Id, false);
            _userConnection.RemoveConnection(connection.Id);

            _logger.LogInformation("Connection {ConnectionId} removed for user {UserId}", connection.Id, user.Id);

            var remainingConnections = _userConnection.GetConnections();
            if (remainingConnections.Count == 0)
            {
                _logger.LogInformation("User {UserId} has no remaining connections. Raising Disconnected event.", user.Id);
                Disconnected?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                NotifyConnectionsChange(connection, user, false);
            }
        }

        private void NotifyConnectionsChange(IConnection changedConnection, User user, bool isOnline)
        {
            var command = new CommandResponse(
                "ConnectionsChange",
                new ConnectionChangeResponse(
                    mapper.ToDTO(_connectionService.GetConnectionInfo(changedConnection.Id, user.Id)),
                    _connectionService.GetConnectionCount(user.Id),
                    isOnline
                )
            );

            _ = Task.Run(async () =>
            {
                try
                {
                    foreach (var connection in _userConnection.GetConnections())
                    {
                        await CommandSender.SendCommandAsync(connection, command, _serializer);
                        _logger.LogInformation("Sent connection change command to connection {ConnectionId} for user {UserId}", connection.Id, user.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending connection change command for user {UserId}", user.Id);
                }
            });
        }

        private void OnCommandGot(object? sender, Command command)
        {
            GotCommand?.Invoke(sender, command);
            _logger.LogInformation("Command received from connection {ConnectionId}", (sender as IConnection)?.Id);
        }

        private void OnConnectionDisconnected(object? sender, User user)
        {
            if (sender is IConnection connection)
            {
                _logger.LogInformation("Connection {ConnectionId} disconnected for user {UserId}", connection.Id, user.Id);
                HandleConnectionRemoved(connection, user);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    GotCommand = null;
                    Disconnected = null;
                    _logger.LogInformation("ConnectionNotifier disposed.");
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
