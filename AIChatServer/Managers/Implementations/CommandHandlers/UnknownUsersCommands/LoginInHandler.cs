using AIChatServer.Entities.Connection;
using AIChatServer.Entities.DTO.Request;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Entities.Exceptions;
using AIChatServer.Entities.User;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Service.Interfaces;
using AIChatServer.Utils;
using AIChatServer.Utils.Interfaces;
using AIChatServer.Utils.Interfaces.Mapper;

namespace AIChatServer.Managers.Implementations.CommandHandlers.UnknownUsersCommands
{
    public class LoginInHandler(IAuthService authService, ISerializer serializer, IResponseMapper<UserBanResponse, UserBan> mapper) : ICommandHandler
    {
        private readonly IAuthService _authService = authService
            ?? throw new ArgumentNullException(nameof(authService));
        private readonly ISerializer _serializer = serializer
            ?? throw new ArgumentNullException(nameof(serializer));
        private readonly IResponseMapper<UserBanResponse, UserBan> _mapper = mapper
            ?? throw new ArgumentNullException(nameof(mapper));

        public string Operation => "LoginIn";

        public async Task HandleAsync(object sender, Command command)
        {
            IUnknownUser userContext = sender as IUnknownUser ?? throw new Exception("Only UnknownUser can be sender");
            AuthRequest authRequest = command.GetData<AuthRequest>() ?? throw new ArgumentNullException("AuthRequest");
            try
            {
                User? user = _authService.Login(authRequest.Email, authRequest.Password);
                if (user != null)
                {
                    if (user.GetRegistrationType() == RegistrationType.Google)
                    {
                        CommandResponse loginInFailed = new CommandResponse("UseOtherLoginInService", new UseOtherLoginInServiceResponse("Google"));
                        await userContext.SendCommandAsync(loginInFailed);
                        return;
                    }
                    userContext.SetUser(user);
                    userContext.KnowUser();
                }
                else
                {
                    await CommandSender.SendCommandAsync(command.Sender, new CommandResponse("LoginInFailed"), _serializer);
                }
            }
            catch (UserBannedException userBannedEx)
            { 
                await userContext.SendCommandAsync(new CommandResponse("BanUser", _mapper.ToDTO(userBannedEx.UserBan)));
            }
        }
    }
}
