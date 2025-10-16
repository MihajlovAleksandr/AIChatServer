using AIChatServer.Entities.Connection.Interfaces;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Managers.Implementations
{
    public class ConnectionStorage(IKnownUserFactory knownUserFactory, IUserService userService,
        ILogger<ConnectionStorage> logger) : IConnectionStorage
    {
        private readonly IKnownUserFactory _knownUserFactory = knownUserFactory
            ?? throw new ArgumentNullException(nameof(knownUserFactory));
        private readonly IUserService _userService = userService 
            ?? throw new ArgumentNullException(nameof(userService));
        private readonly ILogger<ConnectionStorage> _logger = logger 
            ?? throw new ArgumentNullException(nameof(logger));
        private readonly Dictionary<Guid, IServerUser> _users = new();

        public bool Add(Guid userId, IServerUser serverUser)
        {
            if (serverUser == null) throw new ArgumentNullException(nameof(serverUser));

            bool isNewUser = _users.TryAdd(userId, serverUser);
            if (!isNewUser)
            {
                _users[userId].AddConnections(serverUser.GetConnections());
                _logger.LogInformation("Existing user {UserId} added new connections.", userId);
            }
            else
            {
                _logger.LogInformation("New user {UserId} added to storage.", userId);
            }

            return isNewUser;
        }

        public bool Add(Guid userId, IConnection connection, out IServerUser newUser)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var userEntity = _userService.GetUserById(userId);
            if (userEntity == null)
            {
                _logger.LogWarning("Cannot create server user for non-existent user {UserId}.", userId);
                newUser = null!;
                return false;
            }

            newUser = _knownUserFactory.Create(userEntity, connection);
            bool added = Add(userId, newUser);

            _logger.LogInformation("Server user for {UserId} added: {Added}", userId, added);
            return added;
        }

        public bool Remove(Guid userId, out IServerUser? serverUser)
        {
            bool removed = _users.Remove(userId, out serverUser);
            _logger.LogInformation("User {UserId} removed from storage: {Removed}", userId, removed);
            return removed;
        }

        public bool TryGet(Guid userId, out IServerUser? serverUser)
        {
            bool found = _users.TryGetValue(userId, out serverUser);
            _logger.LogDebug("TryGet user {UserId}: {Found}", userId, found);
            return found;
        }
    }
}
