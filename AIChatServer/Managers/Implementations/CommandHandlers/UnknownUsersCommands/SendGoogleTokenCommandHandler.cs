using AIChatServer.Entities.Connection;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Entities.User.ServerUsers;
using AIChatServer.Entities.User;
using AIChatServer.Service.Interfaces;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Entities.DTO.Request;
using Microsoft.Extensions.Logging;
using AIChatServer.Managers.Interfaces;

namespace AIChatServer.Managers.Implementations.CommandHandlers.UnknownUsersCommands
{
    public class SendGoogleTokenCommandHandler(
        IOAuthValidator oAuthValidator,
        IUserService userService,
        IAuthService authService,
        ILogger<SendGoogleTokenCommandHandler> logger) : ICommandHandler
    {
        private readonly IOAuthValidator _oAuthValidator = oAuthValidator 
            ?? throw new ArgumentNullException(nameof(oAuthValidator));
        private readonly IUserService _userService = userService 
            ?? throw new ArgumentNullException(nameof(userService));
        private readonly IAuthService _authService = authService 
            ?? throw new ArgumentNullException(nameof(authService));
        private readonly ILogger<SendGoogleTokenCommandHandler> _logger = logger 
            ?? throw new ArgumentNullException(nameof(logger));

        public string Operation => "SendGoogleTokenCommand";

        public async Task HandleAsync(object sender, Command command)
        {
            if (sender is not IUnknownUser userContext)
            {
                _logger.LogWarning("SendGoogleTokenCommandHandler invoked by non-UnknownUser sender.");
                throw new InvalidOperationException("Only UnknownUser can be sender.");
            }

            var request = command.GetData<GoogleTokenRequest>();
            if (request == null)
            {
                _logger.LogWarning("GoogleTokenRequest is null in command {Command}.", command.Operation);
                throw new ArgumentNullException(nameof(request));
            }

            OAuthUser? oAuthUser;
            try
            {
                oAuthUser = await _oAuthValidator.Validate(request.Token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Google token for command {Command}.", command.Operation);
                return;
            }

            if (oAuthUser == null)
            {
                _logger.LogInformation("Google token validation failed for token {Token}.", request.Token);
                return;
            }

            User? user = _userService.GetUserByEmail(oAuthUser.Email);
            if (user == null)
            {
                user = new User(oAuthUser.Email, oAuthUser.Id, RegistrationType.Google);
                _logger.LogInformation("Registering new Google user with email {Email}.", oAuthUser.Email);
                await userContext.SendCommandAsync(new CommandResponse("GoogleRegistrationSuccess"));
                userContext.SetUser(user);
                return;
            }

            if (user.GetRegistrationType() == RegistrationType.Password)
            {
                _logger.LogInformation("User {Email} has password registration, cannot use Google login.", oAuthUser.Email);
                var loginInFailed = new CommandResponse(
                    "UseOtherLoginInService",
                    new UseOtherLoginInServiceResponse("Password"));
                await userContext.SendCommandAsync(loginInFailed);
                return;
            }

            if (!_authService.VerifyGoogleLogin(oAuthUser.Email, oAuthUser.Id))
            {
                _logger.LogWarning("Google login verification failed for user {Email}.", oAuthUser.Email);
                return;
            }

            userContext.SetUser(user);
            userContext.KnowUser();
            _logger.LogInformation("User {Email} successfully verified and marked as known.", oAuthUser.Email);
        }
    }
}
