using AIChatServer.Entities.Connection;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Entities.User;
using AIChatServer.Service.Interfaces;
using AIChatServer.Entities.DTO.Request;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Utils.Interfaces.Mapper;
using AIChatServer.Managers.Interfaces;

namespace AIChatServer.Managers.Implementations.CommandHandlers.MainManagerCommands
{
    public class UpdateUserDataCommandHandler(IUserService userService,
        IMapper<UserDataRequest, UserData, UserDataResponse> userDataMapper) : ICommandHandler
    {
        private readonly IUserService _userService = userService
            ?? throw new ArgumentNullException(nameof(userService));
        IMapper<UserDataRequest, UserData, UserDataResponse> _userDataMapper = userDataMapper
            ?? throw new ArgumentNullException(nameof(userDataMapper));

        public string Operation => "UpdateUserData";

        public async Task HandleAsync(object sender, Command command)
        {
            var knownUser = (IServerUser)sender;
            UserDataRequest userDataRequest = command.GetData<UserDataRequest>() ?? throw new ArgumentNullException("UserDataRequest");
            UserData userData = _userDataMapper.ToModel(userDataRequest);
            if (_userService.UpdateUserData(userData, knownUser.User.Id))
            {
                knownUser.User.UserData = userData;
                UserDataResponse userDataResponse = _userDataMapper.ToDTO(userData);
                CommandResponse userDataCommand = new CommandResponse("UserDataUpdated", userDataResponse);
                await knownUser.SendCommandAsync(userDataCommand);
            }
        }
    }
}
