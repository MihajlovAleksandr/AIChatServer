using AIChatServer.Entities.Connection;
using AIChatServer.Entities.DTO.Request;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Entities.User;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Utils.Interfaces.Mapper;

namespace AIChatServer.Managers.Implementations.CommandHandlers.UnknownUsersCommands
{
    public class AddUserDataHandler (IRequestMapper<UserDataRequest, UserData> mapper) : ICommandHandler
    {
        private readonly IRequestMapper<UserDataRequest, UserData> _mapper = mapper
            ?? throw new ArgumentNullException(nameof(mapper));

        public string Operation => "AddUserData";

        public async Task HandleAsync(object sender, Command command)
        {
            IUnknownUser userContext = sender as IUnknownUser ?? throw new Exception("Only UnknownUser can be sender");
            UserDataRequest userDataRequest = command.GetData<UserDataRequest>() ?? throw new ArgumentNullException("UserDataRequest");
            userContext.User.UserData = _mapper.ToModel(userDataRequest);
            await userContext.SendCommandAsync(new CommandResponse("UserDataAdded"));
        }
    }
}
