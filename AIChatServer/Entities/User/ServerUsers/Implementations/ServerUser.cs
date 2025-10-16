using AIChatServer.Entities.Connection;
using AIChatServer.Entities.Connection.Interfaces;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Utils;
using AIChatServer.Utils.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Entities.User.ServerUsers.Implementations
{
    public abstract class ServerUser : IServerUser, IDisposable
    {
        private readonly IUserConnection _userConnection;
        private readonly IConnectionNotifier _connectionNotifier;
        private readonly ISerializer _serializer;
        private readonly ILogger<ServerUser> _logger;
        private bool _disposed;

        public User User { get; protected set; }
        public event EventHandler<Command>? GotCommand;
        public event EventHandler? Disconnected;

        protected ServerUser(
            User user,
            IReadOnlyCollection<IConnection> connections,
            ISerializer serializer,
            IUserConnection userConnection,
            IConnectionNotifier connectionNotifier,
            ILogger<ServerUser> logger)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            User = user ?? throw new ArgumentNullException(nameof(user));
            _userConnection = userConnection ?? throw new ArgumentNullException(nameof(userConnection));
            _connectionNotifier = connectionNotifier ?? throw new ArgumentNullException(nameof(connectionNotifier));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _connectionNotifier.Disconnected += OnDisconnected;
            _connectionNotifier.GotCommand += OnCommandGot;

            AddConnections(connections);
            _logger.LogInformation("ServerUser {UserId} created with {Connections} connections.", User.Id, connections.Count);
        }

        protected ServerUser(
            IConnection connection,
            ISerializer serializer,
            IUserConnection userConnection,
            IConnectionNotifier connectionNotifier,
            ILogger<ServerUser> logger)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            User = new User();
            _userConnection = userConnection ?? throw new ArgumentNullException(nameof(userConnection));
            _connectionNotifier = connectionNotifier ?? throw new ArgumentNullException(nameof(connectionNotifier));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _connectionNotifier.Disconnected += OnDisconnected;
            _connectionNotifier.GotCommand += OnCommandGot;

            AddConnection(connection);
            _logger.LogInformation("ServerUser {UserId} created with single connection {ConnectionId}.", User.Id, connection.Id);
        }

        protected abstract bool IsKnownUser();

        public void AddConnection(IConnection connection)
        {
            _userConnection.AddConnection(connection);
            _connectionNotifier.HandleConnectionAdded(connection, User, IsKnownUser());

            _logger.LogDebug("Connection {ConnectionId} added for user {UserId}.", connection.Id, User.Id);
        }

        public void AddConnections(IEnumerable<IConnection> connections)
        {
            foreach (var connection in connections)
            {
                AddConnection(connection);
            }
        }

        public IConnection? RemoveConnection(Guid id)
        {
            var connection = _userConnection.RemoveConnection(id);
            if (connection == null)
            {
                _logger.LogWarning("Attempted to remove non-existing connection {ConnectionId} for user {UserId}.", id, User.Id);
                return null;
            }

            _connectionNotifier.HandleConnectionRemoved(connection, User);
            _logger.LogInformation("Connection {ConnectionId} removed for user {UserId}.", id, User.Id);

            return connection;
        }

        public async Task SendCommandAsync(CommandResponse command)
        {
            _logger.LogDebug("Sending command {Command} to all connections of user {UserId}.", command, User.Id);
            await CommandSender.SendCommandAsync(GetConnections(), command, _serializer);
        }

        public async Task SendCommandAsync(Guid connectionId, CommandResponse command)
        {
            var connection = _userConnection.GetConnection(connectionId);
            if (connection == null)
            {
                _logger.LogWarning("Attempted to send command {Command} to non-existing connection {ConnectionId} for user {UserId}.",
                    command, connectionId, User.Id);
                return;
            }

            _logger.LogDebug("Sending command {Command} to connection {ConnectionId} of user {UserId}.",
                command, connectionId, User.Id);

            await CommandSender.SendCommandAsync(connection, command, _serializer);
        }

        public IReadOnlyCollection<IConnection> GetConnections() => _userConnection.GetConnections();

        public IConnection? GetConnection(Guid id) => _userConnection.GetConnection(id);

        protected virtual void OnCommandGot(object? sender, Command command)
        {
            _logger.LogTrace("Command {CommandType} received from user {UserId}.", command.Operation, User.Id);
            GotCommand?.Invoke(this, command);
            if (GotCommand == null) _logger.LogError("The command {command} was not processed due to GotCommand == null", command.Operation);
        }

        protected virtual void OnDisconnected(object? sender, EventArgs e)
        {
            _logger.LogInformation("User {UserId} disconnected.", User.Id);
            Disconnected?.Invoke(this, e);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _connectionNotifier.Disconnected -= OnDisconnected;
                _connectionNotifier.GotCommand -= OnCommandGot;
                _logger.LogInformation("ServerUser {UserId} disposed.", User.Id);
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
