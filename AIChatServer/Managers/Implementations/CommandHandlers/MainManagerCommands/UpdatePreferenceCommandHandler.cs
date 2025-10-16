using AIChatServer.Entities.Connection;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Entities.User;
using AIChatServer.Service.Interfaces;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Entities.DTO.Request;
using AIChatServer.Utils.Interfaces.Mapper;
using AIChatServer.Managers.Interfaces;

namespace AIChatServer.Managers.Implementations.CommandHandlers.MainManagerCommands
{
    public class UpdatePreferenceCommandHandler(IUserService userService,
        IMapper<PreferenceRequest, Preference, PreferenceResponse> preferenceMapper) : ICommandHandler
    {
        private readonly IUserService _userService = userService 
            ?? throw new ArgumentNullException(nameof(userService));
        private readonly IMapper<PreferenceRequest, Preference, PreferenceResponse> _preferenceMapper = preferenceMapper
            ?? throw new ArgumentNullException(nameof(preferenceMapper));

        public string Operation => "UpdatePreference";

        public async Task HandleAsync(object sender, Command command)
        {
            var knownUser = (IServerUser)sender;
            PreferenceRequest preferenceRequest = command.GetData<PreferenceRequest>() ?? throw new ArgumentNullException("PreferenceRequest");
            Preference preference = _preferenceMapper.ToModel(preferenceRequest);
            if (_userService.UpdatePreference(preference, knownUser.User.Id))
            {
                PreferenceResponse preferenceResponse = _preferenceMapper.ToDTO(preference);
                knownUser.User.Preference = preference;
                await knownUser.SendCommandAsync(new CommandResponse("PreferenceUpdated", preferenceResponse));
            }
        }
    }
}
