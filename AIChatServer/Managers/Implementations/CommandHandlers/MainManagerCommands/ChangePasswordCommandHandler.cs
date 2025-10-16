using AIChatServer.Entities.Connection;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Service.Interfaces;
using AIChatServer.Utils;
using AIChatServer.Utils.Interfaces;
using AIChatServer.Entities.DTO.Request;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Managers.Interfaces;

namespace AIChatServer.Managers.Implementations.CommandHandlers.MainManagerCommands
{
    public class ChangePasswordCommandHandler(IAuthService authService, ISerializer serializer) : ICommandHandler
    {
        private readonly IAuthService _authService = authService 
            ?? throw new ArgumentNullException(nameof(authService));
        private readonly ISerializer _serializer = serializer 
            ?? throw new ArgumentNullException(nameof(serializer));

        public string Operation => "ChangePassword";

        public async Task HandleAsync(object sender, Command command)
        {
            var knownUser = (IServerUser)sender;

            ChangePasswordRequest changePasswordRequest = command.GetData<ChangePasswordRequest>()
                ?? throw new ArgumentNullException(nameof(changePasswordRequest));

            if (_authService.VerifyPassword(knownUser.User.Password, changePasswordRequest.CurrentPassword))
            {
                string newPassword = _authService.ChangePassword(knownUser.User.Id, changePasswordRequest.NewPassword);
                if (!string.IsNullOrEmpty(newPassword))
                {
                    await CommandSender.SendCommandAsync(command.Sender, new CommandResponse("PasswordChanged"), _serializer);
                    knownUser.User.Password = newPassword;
                }
            }
        }
    }
}
