using AIChatServer.Config.Data;
using AIChatServer.Entities.User.ServerUsers.Implementations;
using AIChatServer.Factories.Containers;
using AIChatServer.Factories.Interfaces;
using AIChatServer.Managers.Implementations.CommandHandlers.UnknownUsersCommands;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Utils.Interfaces;

namespace AIChatServer.Factories.Implementations
{
    public class UserFactoryFactory(ISerializer serializer, IVerificationCodeSender verificationCodeSender,
        ICompositeLoggerFactory compositeLoggerFactory, Random random) : IUserFactoryFactory
    {
        private readonly ISerializer _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        private readonly IVerificationCodeSender _verificationCodeSender = verificationCodeSender ?? throw new ArgumentNullException(nameof(verificationCodeSender));
        private readonly ICompositeLoggerFactory _compositeLoggerFactory = compositeLoggerFactory ?? throw new ArgumentNullException(nameof(compositeLoggerFactory));
        private readonly Random _random = random ?? throw new ArgumentNullException(nameof(random));

        public UserFactoryContainer CreateUserFactories(ServiceContainer services, TokenManagerContainer tokenManagers,
            MapperContainer mappers, ConfigData configData)
        {
            var knownUserFactory = new KnownUserFactory(
                services.ConnectionService,
                _serializer,
                mappers.ConnectionInfoMapper,
                _compositeLoggerFactory.Create<ConnectionNotifier>(),
                _compositeLoggerFactory.Create<UserConnection>(),
                _compositeLoggerFactory.Create<ServerUser>());

            var unknownHandlers = new List<ICommandHandler>()
            {
                new AddPreferenceHandler(services.UserService, mappers.PreferenceRequestMapper),
                new AddUserDataHandler(mappers.UserDataRequestMapper),
                new GetEntryTokenHandler(_serializer, tokenManagers.EntryTokenManager),
                new LoginInHandler(services.AuthService, _serializer, mappers.UserBanResponseMapper),
                new RegistrationHandler(
                    services.UserService,
                    _verificationCodeSender,
                    _random),
                new SendGoogleTokenCommandHandler(
                    new GoogleAuthValidator(configData.GoogleConfigData.ClientId, _compositeLoggerFactory.Create<GoogleAuthValidator>()),
                    services.UserService,
                    services.AuthService,
                    _compositeLoggerFactory.Create<SendGoogleTokenCommandHandler>()),
                new VerificationCodeHandler()
            };

            var unknownUserFactory = new UnknownUserFactory(
                services.ConnectionService,
                _serializer,
                unknownHandlers,
                mappers.ConnectionInfoMapper,
                _compositeLoggerFactory.Create<ConnectionNotifier>(),
                _compositeLoggerFactory.Create<UserConnection>(),
                _compositeLoggerFactory.Create<ServerUser>());

            return new UserFactoryContainer(knownUserFactory, unknownUserFactory);
        }
    }
}
