using AIChatServer.Config.Data;
using AIChatServer.Entities.Chats;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Factories.Interfaces;
using AIChatServer.Managers.Implementations;
using AIChatServer.Utils.Implementations.Mappers;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Entities.AI.Implementations;
using AIChatServer.Integrations.AI.Implementations;
using AIChatServer.Utils.Interfaces;
using AIChatServer.Entities.Connection.Interfaces;
using AIChatServer.Factories.Containers;

namespace AIChatServer.Factories.Implementations
{
    public class ManagerFactory(ISerializer serializer, ICompositeLoggerFactory loggerFactory, IHttpService httpService) : IManagerFactory
    {
        private readonly ISerializer _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        private readonly ICompositeLoggerFactory _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        private readonly IHttpService _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));

        public async Task<ManagerContainer> CreateManagers(ConfigData configData, MapperContainer mappers, Dictionary<ChatType, IChatMatchStrategy> strategies,
            TokenManagerContainer tokenManagers, ServiceContainer serviceContainer, UserFactoryContainer userFactoryContainer, IConnectionFactory connectionFactory)
        {
            var deepSeekController = new DeepSeekController(
                configData.DeepSeekConfigData.MaxTokenCount,
                _serializer,
                _httpService,
                configData.DeepSeekConfigData.ApiKey,
                configData.DeepSeekConfigData.ApiUrl,
                _loggerFactory.Create<DeepSeekController>());

            var aiDispatcherFactory = new AIMessageDispatcherFactory(
                (configData.AIMessageBufferData.MessageBufferSize.Max, configData.AIMessageBufferData.MessageBufferSize.Min),
                (configData.AIMessageBufferData.CompressedMessageBufferSize.Max, configData.AIMessageBufferData.CompressedMessageBufferSize.Min),
                _loggerFactory.Create<AIMessageDispatcher>(),
                new AIMessageGroupFactory(_loggerFactory.Create<AIMessageGroup>()));



            var tokenService = new TokenManager(tokenManagers.ConnectionTokenManager, _loggerFactory.Create<TokenManager>());
            var notificationManager = new FirebaseNotificationManager("aichatFirebase.json", _loggerFactory.Create<FirebaseNotificationManager>());
            var chatManager = new ChatManager(serviceContainer.ChatService, strategies, _loggerFactory.Create<ChatManager>());

            var connectionStorage = new ConnectionStorage(userFactoryContainer.KnownUserFactory, serviceContainer.UserService,
                _loggerFactory.Create<ConnectionStorage>());

            var connectionManager = new ConnectionManager(connectionStorage,
                new UserValidator(new TokenRefresher(_serializer, tokenManagers.ConnectionTokenManager), 
                _loggerFactory.Create<UserValidator>()),
                _loggerFactory.Create<ConnectionManager>());

            var userManager = new UserManager(
            connectionManager,
                serviceContainer.ConnectionService,
                userFactoryContainer.KnownUserFactory,
                userFactoryContainer.UnknownUserFactory,
            tokenService,
                _loggerFactory.Create<UserManager>());

            var messageCollectionMapper = new CollectionResponseMapper<MessageResponse, Message>(mappers.MessageMapper);
            var chatCollectionMapper = new CollectionResponseMapper<ChatResponse, ChatWithUserContext>(mappers.ChatResponseMapper);

            var syncService = new SyncManager(serviceContainer.ChatService, chatManager.IsSearchingChat, messageCollectionMapper,
            chatCollectionMapper, _loggerFactory.Create<SyncManager>());

            var aiMessages = await serviceContainer.AIMessageService.GetAIMessagesByChatAsync(chatManager.GetUserChats(configData.AIConfigData.AIId));

            var aiManager = new AIManager(
                configData.AIConfigData.AIId,
                deepSeekController,
                serviceContainer.ChatService,
                serviceContainer.AIMessageService,
                mappers.AIMessageMapper,
                aiMessages,
                aiDispatcherFactory,
                _loggerFactory.Create<AIManager>());

            var clientHandler = new ClientHandler(
                userManager,
                 serviceContainer.ConnectionService,
                _serializer,
                tokenService,
                connectionManager,
                 syncService,
                 connectionFactory,
                 mappers.UserBanResponseMapper,
                 _loggerFactory.Create<ClientHandler>());

            var connectionListener = new ConnectionListener(configData.ServerConfigData.ServerURL, clientHandler,
                _loggerFactory.Create<ConnectionListener>());


            return new ManagerContainer(
                chatManager, userManager, notificationManager, syncService, aiManager, clientHandler, connectionListener
            );
        }
    }
}
