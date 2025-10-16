using AIChatServer.Entities.Connection;
using AIChatServer.Entities.Connection.Interfaces;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Service.Interfaces;
using AIChatServer.Utils.Interfaces;
using AIChatServer.Utils.Interfaces.Mapper;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Entities.User.ServerUsers.Implementations
{
    public class UnknownUserFactory(IConnectionService connectionService, ISerializer serializer,
        IReadOnlyCollection<ICommandHandler> commandHandlers,
        IResponseMapper<ConnectionInfoResponse, ConnectionInfo> mapper,
        ILogger<ConnectionNotifier> connectionNotifierLogger,
        ILogger<UserConnection> userConnectionLogger, ILogger<ServerUser> serverUserLogger) : IUnknownUserFactory
    {
        private readonly IConnectionService _connectionService = connectionService
            ?? throw new ArgumentNullException(nameof(connectionService));
        private readonly ISerializer _serializer = serializer
            ?? throw new ArgumentNullException(nameof(serializer));
        private readonly IReadOnlyCollection<ICommandHandler> _commandHandlers = commandHandlers
            ?? throw new ArgumentNullException(nameof(commandHandlers));
        private readonly IResponseMapper<ConnectionInfoResponse, ConnectionInfo> _mapper = mapper
            ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<ConnectionNotifier> _connectionNotifierLogger = connectionNotifierLogger
            ?? throw new ArgumentNullException(nameof(connectionNotifierLogger));
        private readonly ILogger<UserConnection> _userConnectionLogger = userConnectionLogger
            ?? throw new ArgumentNullException(nameof(userConnectionLogger));
        private readonly ILogger<ServerUser> _serverUserLogger = serverUserLogger 
            ?? throw new ArgumentNullException(nameof(serverUserLogger));

        public UnknownUser Create(Guid id, IConnection connection)
        {
            var userConnection = new UserConnection(_userConnectionLogger);
            var connectionNotifier = new ConnectionNotifier(_connectionService, userConnection, _serializer, _mapper, _connectionNotifierLogger);

            return new UnknownUser(
                id,
                _commandHandlers,
                connection,
                _connectionService,
                _serializer,
                userConnection,
                connectionNotifier, 
                _serverUserLogger
            );
        }
    }
}
