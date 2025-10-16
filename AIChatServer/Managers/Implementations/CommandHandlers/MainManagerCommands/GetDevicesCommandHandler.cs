using AIChatServer.Entities.Connection;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Service.Interfaces;
using AIChatServer.Utils;
using AIChatServer.Utils.Interfaces;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Utils.Interfaces.Mapper;
using AIChatServer.Managers.Interfaces;

namespace AIChatServer.Managers.Implementations.CommandHandlers.MainManagerCommands
{
    public class GetDevicesCommandHandler(IConnectionService connectionService, ISerializer serializer, ICollectionResponseMapper<ConnectionInfoResponse, ConnectionInfo> responseMapper) : ICommandHandler
    {
        private readonly IConnectionService _connectionService = connectionService
            ?? throw new ArgumentNullException(nameof(connectionService));
        private readonly ISerializer _serializer = serializer
            ?? throw new ArgumentNullException(nameof(serializer));
        private readonly ICollectionResponseMapper<ConnectionInfoResponse, ConnectionInfo> responseMapper = responseMapper
            ?? throw new ArgumentNullException(nameof(responseMapper));

        public string Operation => "GetDevices";

        public async Task HandleAsync(object sender, Command command)
        {
            var knownUser = (IServerUser)sender;
            var devicesList = _connectionService.GetAllUserConnections(knownUser.User.Id);
            CommandResponse devicesCommand = new CommandResponse("GetDevices", new DeviceResponse(responseMapper.ToDTO(devicesList), command.Sender.Id));
            await CommandSender.SendCommandAsync(command.Sender, devicesCommand, _serializer);
        }
    }
}
