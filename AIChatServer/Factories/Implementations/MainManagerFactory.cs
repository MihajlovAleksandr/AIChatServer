using AIChatServer.Config.Data;
using AIChatServer.Entities.Connection.Interfaces;
using AIChatServer.Factories.Containers;
using AIChatServer.Factories.Interfaces;
using AIChatServer.Managers;
using AIChatServer.Managers.Interfaces;

namespace AIChatServer.Factories.Implementations
{
    public class MainManagerFactory(
        IMapperFactory mapperFactory,
        ITokenManagerFactory tokenManagerFactory,
        IRepositoryFactory repositoryFactory,
        IServiceFactory serviceFactory,
        IManagerFactory managerFactory,
        IUserFactoryFactory userFactoryFactory,
        ICommandHandlerFactory commandHandlerFactory,
        IChatStrategyFactory chatStrategyFactory,
        ICompositeLoggerFactory loggerFactory) : IMainManagerFactory
    {
        private readonly IMapperFactory _mapperFactory = mapperFactory ?? throw new ArgumentNullException(nameof(mapperFactory));
        private readonly ITokenManagerFactory _tokenManagerFactory = tokenManagerFactory ?? throw new ArgumentNullException(nameof(tokenManagerFactory));
        private readonly IRepositoryFactory _repositoryFactory = repositoryFactory ?? throw new ArgumentNullException(nameof(repositoryFactory));
        private readonly IServiceFactory _serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
        private readonly IManagerFactory _managerFactory = managerFactory ?? throw new ArgumentNullException(nameof(managerFactory));
        private readonly IUserFactoryFactory _userFactoryFactory = userFactoryFactory ?? throw new ArgumentNullException(nameof(userFactoryFactory));
        private readonly ICommandHandlerFactory _commandHandlerFactory = commandHandlerFactory ?? throw new ArgumentNullException(nameof(commandHandlerFactory));
        private readonly IChatStrategyFactory _chatStrategyFactory = chatStrategyFactory ?? throw new ArgumentNullException(nameof(chatStrategyFactory));
        private readonly ICompositeLoggerFactory _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

        public async Task<MainManagerDependencies> Create(ConfigData configData, IConnectionFactory connectionFactory)
        {
            ArgumentNullException.ThrowIfNull(configData);

            var repoContainer = _repositoryFactory.CreateRepositories(configData);

            var mapperContainer = _mapperFactory.CreateMappers();

            var tokenManagerContainer = _tokenManagerFactory.CreateTokenManagers(configData);

            var chatStrategies = _chatStrategyFactory.CreateStrategies(configData.AIConfigData.AIId);

            var serviceContainer = _serviceFactory.CreateServices(configData, repoContainer);

            var userFactories = _userFactoryFactory.CreateUserFactories(serviceContainer, tokenManagerContainer, mapperContainer, configData);

            var managerContainer = await _managerFactory.CreateManagers(configData, mapperContainer, chatStrategies, tokenManagerContainer,
                serviceContainer, userFactories, connectionFactory);

            var commandHandlers = _commandHandlerFactory.CreateCommandHandlers(serviceContainer, tokenManagerContainer, managerContainer,
                mapperContainer);

            var userEvents = managerContainer.UserEvents;
            var connectionListener = managerContainer.ConnectionListener;

            var deps = new MainManagerDependencies(
                ChatManager: managerContainer.ChatManager,
                UserManager: managerContainer.UserManager,
                AIManager: managerContainer.AIManager,
                NotificationManager: managerContainer.NotificationManager,
                ChatService: serviceContainer.ChatService,
                MessageService: serviceContainer.MessageService,
                UserService: serviceContainer.UserService,
                NotificationService: serviceContainer.NotificationService,
                SendCommandMapper: mapperContainer.SendCommandMapper,
                ChatResponseMapper: mapperContainer.ChatResponseMapper,
                MessageMapper: mapperContainer.MessageMapper,
                CommandHandlers: commandHandlers,
                Logger: _loggerFactory.Create<MainManager>(),
                UserEvents: userEvents,
                ConnectionListener: connectionListener
            );

            return deps;
        }
    }
}
