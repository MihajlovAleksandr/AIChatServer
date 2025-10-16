using AIChatServer.Entities.Connection;
using AIChatServer.Entities.DTO.Request;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Entities.User.ServerUsers;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Managers.Interfaces;

namespace AIChatServer.Managers.Implementations.CommandHandlers.UnknownUsersCommands
{
    public class VerificationCodeHandler : ICommandHandler
    {
        public string Operation => "VerificationCode";

        public async Task HandleAsync(object sender, Command command)
        {
            IUnknownUser userContext = sender as IUnknownUser ?? throw new Exception("Only UnknownUser can be sender");
            VerificationCode? verificationCode = userContext.GetTempData<VerificationCode>("verificationCode");
            if(verificationCode == null)
            {
                await userContext.SendCommandAsync(new CommandResponse("Logout"));
                return;
            }
            VerificationCodeRequest request = command.GetData<VerificationCodeRequest>() ?? throw new ArgumentNullException("VerificationCodeRequest");
            CommandResponse returnCommand = new CommandResponse("VerificationCodeAnswer",
                new VerificationCodeResponse(verificationCode.Validate(request.Code)));
            await userContext.SendCommandAsync(returnCommand);
        }
    }
}
