using AIChatServer.Entities.Connection;
using AIChatServer.Entities.Connection.Interfaces;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Entities.User.ServerUsers.Implementations;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Managers.Implementations
{
    public class UserManager : IUserManager
    {
        private readonly IConnectionManager _connectionManager;
        private readonly IConnectionService _connectionService;
        private readonly IKnownUserFactory _knownUserFactory;
        private readonly IUnknownUserFactory _unknownUserFactory;
        private readonly ITokenManager _tokenService;
        private readonly ILogger<UserManager> _logger;

        public event EventHandler<Command> CommandGot;
        public event EventHandler<bool> OnConnectionEvent;

        public UserManager(
            IConnectionManager connectionManager,
            IConnectionService connectionService,
            IKnownUserFactory knownUserFactory,
            IUnknownUserFactory unknownUserFactory,
            ITokenManager tokenService,
            ILogger<UserManager> logger)
        {
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _connectionService = connectionService ?? throw new ArgumentNullException(nameof(connectionService));
            _knownUserFactory = knownUserFactory ?? throw new ArgumentNullException(nameof(knownUserFactory));
            _unknownUserFactory = unknownUserFactory ?? throw new ArgumentNullException(nameof(unknownUserFactory));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task KnowUserAsync(Guid unknownUserId, IServerUser knownUser)
        {
            _logger.LogInformation("Knowing user {UnknownUserId} as {KnownUserId}", unknownUserId, knownUser.User.Id);

            IServerUser? user = _connectionManager.GetUser(unknownUserId)
                ?? throw new ArgumentException($"No user with id = {unknownUserId}");

            IConnection? conn = user.GetConnections().FirstOrDefault()
                ?? throw new ArgumentException("Connection was null");

            _connectionService.UpdateConnection(conn.Id, knownUser.User.Id);
            _logger.LogInformation("Updated connection {ConnectionId} to user {UserId}", conn.Id, knownUser.User.Id);

            if (_connectionManager.ReconnectUser(unknownUserId, knownUser.User.Id))
            {
                knownUser.GotCommand += (s, c) => CommandGot?.Invoke(s, c);
                knownUser.Disconnected += (s, e) => DisconnectUser(s);

                _logger.LogInformation("Reconnected unknown user {UnknownUserId} to known user {KnownUserId}", unknownUserId, knownUser.User.Id);
            }

            await SendLoginCommandsAsync(conn.Id, user, knownUser.User.Id);
        }

        public async Task<IUnknownUser> CreateUnknownUser(IConnection connection)
        {
            Guid userId = Guid.NewGuid();
            UnknownUser unknownUser = _unknownUserFactory.Create(userId, connection);

            unknownUser.UserChanged += u => _ = KnowUserAsync(u.Id, _knownUserFactory.Create(u.User, u.GetConnections()));
            unknownUser.Disconnected += (s, e) => DisconnectUser(s);

            await unknownUser.SendCommandAsync(new CommandResponse("Logout"));
            _connectionManager.ConnectUser(userId, unknownUser);
            _logger.LogInformation("Created unknown user {UserId} and connected", userId);

            return unknownUser;
        }

        public async Task SendCommandAsync(IDictionary<Guid, CommandResponse> userCommandPairs)
        {
            foreach (var userCommandPair in userCommandPairs)
            {
                IServerUser? user = _connectionManager.GetUser(userCommandPair.Key);
                if (user != null)
                {
                    await user.SendCommandAsync(userCommandPair.Value);
                    _logger.LogInformation("Sent command {CommandName} to user {UserId}", userCommandPair.Value, userCommandPair.Key);
                }
                else
                {
                    _logger.LogWarning("User {UserId} not found to send command {CommandName}", userCommandPair.Key, userCommandPair.Value);
                }
            }
        }

        private async Task SendLoginCommandsAsync(Guid connId, IServerUser user, Guid newUserId)
        {
            CommandResponse tokenCmd = new("CreateToken", new TokenResponse(_tokenService.GenerateToken(newUserId, connId)));
            await user.SendCommandAsync(tokenCmd);
            _logger.LogInformation("Sent CreateToken command to user {UserId}", newUserId);

            CommandResponse loginCmd = new("LoginIn", new LoginInResponse(newUserId));
            await user.SendCommandAsync(loginCmd);
            _logger.LogInformation("Sent LoginIn command to user {UserId}", newUserId);
        }

        private void DisconnectUser(object sender)
        {
            if (sender is IServerUser su)
            {
                _connectionManager.DisconnectUser(su.User.Id, out _);
                OnConnectionEvent?.Invoke(sender, false);
                _logger.LogInformation("Disconnected user {UserId}", su.User.Id);
            }
        }
    }
}
