using AIChatServer.Entities.Connection;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Service.Interfaces;
using AIChatServer.Utils;
using AIChatServer.Utils.Interfaces;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Entities.User;
using AIChatServer.Utils.Interfaces.Mapper;
using AIChatServer.Managers.Interfaces;

namespace AIChatServer.Managers.Implementations.CommandHandlers.MainManagerCommands
{
    public class GetSettingsInfoCommandHandler(IConnectionService connectionService,
        INotificationService notificationService, ISerializer serializer, IResponseMapper<UserDataResponse, UserData> userdataMapper,
        IResponseMapper<PreferenceResponse, Preference> preferenceMapper) : ICommandHandler
    {
        private readonly IConnectionService _connectionService = connectionService 
            ?? throw new ArgumentNullException(nameof(connectionService));
        private readonly INotificationService _notificationService = notificationService 
            ?? throw new ArgumentNullException(nameof(notificationService));
        private readonly ISerializer _serializer = serializer 
            ?? throw new ArgumentNullException(nameof(serializer));
        private readonly IResponseMapper<UserDataResponse, UserData> _userdataMapper = userdataMapper 
            ?? throw new ArgumentNullException(nameof(userdataMapper));
        private readonly IResponseMapper<PreferenceResponse, Preference> _preferenceMapper = preferenceMapper 
            ?? throw new ArgumentNullException(nameof(preferenceMapper));

        public string Operation => "GetSettingsInfo";

        public async Task HandleAsync(object sender, Command command)
        {
            var knownUser = (IServerUser)sender;
            CommandResponse getSettingsInfoCommand = new CommandResponse("GetSettingsInfo", new SettingsInfoResponse(knownUser.User.Email,
                _userdataMapper.ToDTO(knownUser.User.UserData), _preferenceMapper.ToDTO(knownUser.User.Preference),
                _connectionService.GetConnectionCount(knownUser.User.Id),
                new NotificationResponse(_notificationService.GetNotifications(knownUser.User.Id))));
            await CommandSender.SendCommandAsync(command.Sender, getSettingsInfoCommand, _serializer);
        }
    }
}
