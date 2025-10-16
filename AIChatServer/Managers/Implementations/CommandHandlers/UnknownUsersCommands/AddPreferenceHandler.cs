using AIChatServer.Entities.Connection;
using AIChatServer.Entities.DTO.Request;
using AIChatServer.Entities.User;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Service.Interfaces;
using AIChatServer.Utils.Interfaces.Mapper;

namespace AIChatServer.Managers.Implementations.CommandHandlers.UnknownUsersCommands
{
    public class AddPreferenceHandler(IUserService userService, IRequestMapper<PreferenceRequest, Preference> requestMapper) : ICommandHandler
    {
        private readonly IUserService _userService = userService
            ?? throw new ArgumentNullException(nameof(userService));
        private readonly IRequestMapper<PreferenceRequest, Preference> _requestMapper = requestMapper
            ?? throw new ArgumentNullException(nameof(requestMapper));

        public string Operation => "AddPreference";

        public Task HandleAsync(object sender, Command command)
        {
            IUnknownUser userContext = sender as IUnknownUser ?? throw new Exception("Only UnknownUser can be sender");
            PreferenceRequest? preferenceRequest = command.GetData<PreferenceRequest>();
            Preference preference;
            if (preferenceRequest == null)
                preference = new Preference() { MinAge = 18, MaxAge = 120, Gender = PreferenceGender.Any };
            else
                preference = _requestMapper.ToModel(preferenceRequest);
            userContext.User.Preference = preference;
            Guid? userId = _userService.AddUser(userContext.User, command.Sender.Id);
            if (userId != null)
            {
                userContext.User.Id = (Guid)userId;
                userContext.KnowUser();
            }
            return Task.CompletedTask;
        }
    }
}
