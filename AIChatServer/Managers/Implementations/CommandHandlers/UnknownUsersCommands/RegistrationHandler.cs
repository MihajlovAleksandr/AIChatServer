using AIChatServer.Entities.Connection;
using AIChatServer.Entities.User.ServerUsers;
using AIChatServer.Entities.User;
using AIChatServer.Utils.Interfaces;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Service.Interfaces;
using AIChatServer.Entities.DTO.Request;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Managers.Interfaces;

namespace AIChatServer.Managers.Implementations.CommandHandlers.UnknownUsersCommands
{
    public class RegistrationHandler(IUserService userService, IVerificationCodeSender verificationCodeSender, Random random) : ICommandHandler
    {
        private readonly IUserService _userService = userService
            ?? throw new ArgumentNullException(nameof(userService));
        private readonly IVerificationCodeSender _verificationCodeSender = verificationCodeSender
            ?? throw new ArgumentNullException(nameof(verificationCodeSender));
        private readonly Random _random = random
            ?? throw new ArgumentNullException(nameof(random));

        public string Operation => "Registration";

        public async Task HandleAsync(object sender, Command command)
        {
            IUnknownUser userContext = sender as IUnknownUser ?? throw new Exception("Only UnknownUser can be sender");
            RegistrationRequest  registrationRequest = command.GetData<RegistrationRequest>() ?? throw new ArgumentNullException("AuthReaquest");
            if (_userService.IsEmailFree(registrationRequest.Email))
            {
                userContext.SetUser(new User(registrationRequest.Email, registrationRequest.Password, RegistrationType.Password));
                VerificationCode verificationCode = new(_random);
                userContext.StoreTempData("verificationCode", verificationCode);
                Console.WriteLine(verificationCode.Code);
                _verificationCodeSender.Send(registrationRequest.Email, verificationCode.Code, registrationRequest.Localization);
                await userContext.SendCommandAsync(new CommandResponse("VerificationCodeSend"));
            }
            else
            {
                await userContext.SendCommandAsync(new CommandResponse("EmailIsBusy"));
            }
        }
    }
}
