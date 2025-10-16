using AIChatServer.Entities.Connection.Interfaces;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Managers.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Managers.Implementations
{
    public class ConnectionManager : IConnectionManager
    {
        private readonly IConnectionStorage _connectionStorage;
        private readonly IUserValidator _userValidator;
        private readonly ILogger<ConnectionManager> _logger;
        private readonly object _syncObj = new();

        public ConnectionManager(
            IConnectionStorage connectionStorage,
            IUserValidator userValidator,
            ILogger<ConnectionManager> logger)
        {
            _connectionStorage = connectionStorage ?? throw new ArgumentNullException(nameof(connectionStorage));
            _userValidator = userValidator ?? throw new ArgumentNullException(nameof(userValidator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool ConnectUser(Guid userId, IServerUser serverUser)
        {
            lock (_syncObj)
            {
                bool added = _connectionStorage.Add(userId, serverUser);
                _logger.LogInformation("User {UserId} connection attempt: {Result}", userId, added ? "Connected" : "Already connected");
                return added;
            }
        }

        public IServerUser? GetUser(Guid userId)
        {
            bool exists = _connectionStorage.TryGet(userId, out IServerUser? serverUser);
            _logger.LogDebug("GetUser {UserId}: {Found}", userId, exists);
            return serverUser;
        }

        public bool DisconnectUser(Guid id, out IServerUser? serverUser)
        {
            lock (_syncObj)
            {
                bool removed = _connectionStorage.Remove(id, out serverUser);
                _logger.LogInformation("User {UserId} disconnected: {Result}", id, removed ? "Success" : "Not found");
                return removed;
            }
        }

        public bool ReconnectUser(Guid oldId, Guid newUserId)
        {
            lock (_syncObj)
            {
                if (_connectionStorage.Remove(oldId, out IServerUser? oldUser))
                {
                    bool added = _connectionStorage.Add(newUserId, oldUser);
                    _logger.LogInformation("User {OldId} reconnected as {NewId}: {Result}", oldId, newUserId, added ? "Success" : "Failed");
                    return added;
                }
                else
                {
                    _logger.LogWarning("Reconnect attempt failed: no user with id {OldId}", oldId);
                    throw new ArgumentException($"No user with id = {oldId}");
                }
            }
        }

        public async Task<IServerUser?> СreateUserAsync(Guid userId, bool needToRefreshToken, IConnection connection)
        {
            if (!await _userValidator.ValidateAsync(userId, needToRefreshToken, connection))
            {
                _logger.LogWarning("User {UserId} failed validation during creation.", userId);
                return null;
            }

            lock (_syncObj)
            {
                _connectionStorage.Add(userId, connection, out IServerUser serverUser);
                _logger.LogInformation("User {UserId} created and added to storage.", userId);
                return serverUser;
            }
        }

        public IServerUser[] GetConnectedUsers(Guid[] users)
        {
            lock (_syncObj)
            {
                List<IServerUser> serverUsers = new();
                foreach (var userId in users)
                {
                    if (_connectionStorage.TryGet(userId, out IServerUser? serverUser))
                    {
                        serverUsers.Add(serverUser);
                    }
                }
                _logger.LogDebug("GetConnectedUsers called for {UserCount} users, found {ConnectedCount}.", users.Length, serverUsers.Count);
                return serverUsers.ToArray();
            }
        }
    }
}
