using AIChatServer.Entities.Connection.Interfaces;
using AIChatServer.Entities.Connection;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Service.Interfaces;
using AIChatServer.Entities.DTO.Request;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Utils.Interfaces.Mapper;

namespace AIChatServer.Managers.Implementations.CommandHandlers.MainManagerCommands
{
    public class DeleteConnectionCommandHandler(IConnectionService connectionService, IUserManager userManager,
        IResponseMapper<ConnectionInfoResponse, ConnectionInfo> mapper) : ICommandHandler
    {
        private readonly IConnectionService _connectionService = connectionService ?? throw new ArgumentNullException(nameof(connectionService));
        private readonly IUserManager _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly IResponseMapper<ConnectionInfoResponse, ConnectionInfo> _mapper = mapper
            ?? throw new ArgumentNullException(nameof(mapper));

        public string Operation => "DeleteConnection";

        public async Task HandleAsync(object sender, Command command)
        {
            var knownUser = (IServerUser)sender;
            DeleteConnectionRequest deleteConnectionRequest= command.GetData<DeleteConnectionRequest>()
                ?? throw new ArgumentNullException(nameof(deleteConnectionRequest));
            Guid connectionId = deleteConnectionRequest.ConnectionId ?? command.Sender.Id;

            ConnectionInfo connectionInfo = _connectionService.RemoveConnection(connectionId);
            if (connectionInfo != null)
            {
                await knownUser.SendCommandAsync(connectionId, new CommandResponse("Logout"));
                IConnection? connection = knownUser.RemoveConnection(connectionId);

                if (connection is not null)
                    await _userManager.CreateUnknownUser(connection);
                else
                    _connectionService.DeleteUnknownConnection(connectionId);

                await knownUser.SendCommandAsync(new CommandResponse("DeleteConnection", 
                    new DeleteConnectionResponse(_mapper.ToDTO(connectionInfo), _connectionService.GetConnectionCount(connectionInfo.UserId))));
            }
        }
    }
}
