using AIChatServer.Config.Data;
using AIChatServer.Entities.AI.Implementations;
using AIChatServer.Factories.Containers;
using AIChatServer.Factories.Interfaces;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Service.Implementations;

namespace AIChatServer.Factories.Implementations
{
    public class ServiceFactory(ICompositeLoggerFactory loggerFactory) : IServiceFactory
    {
        private readonly ICompositeLoggerFactory _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

        public ServiceContainer CreateServices(ConfigData configData, RepositoryContainer repos)
        {
            var chatService = new ChatService(repos.ChatRepository, _loggerFactory.Create<ChatService>());
            var messageService = new MessageService(repos.MessageRepository, _loggerFactory.Create<MessageService>());
            var userService = new UserService(repos.UserRepository, _loggerFactory.Create<UserService>());
            var connectionService = new ConnectionService(repos.ConnectionRepository, _loggerFactory.Create<ConnectionService>());
            var authService = new AuthService(repos.AuthRepository, new Hasher(_loggerFactory.Create<Hasher>()),
                _loggerFactory.Create<AuthService>());
            var notificationService = new NotificationService(repos.NotificationRepository, _loggerFactory.Create<NotificationService>());
            var aiMessageService = new AIMessageService(
                repos.AIMessageRepository,
                new AIMessageDispatcherFactory(
                    (configData.AIMessageBufferData.MessageBufferSize.Max, configData.AIMessageBufferData.MessageBufferSize.Min),
                    (configData.AIMessageBufferData.CompressedMessageBufferSize.Max, configData.AIMessageBufferData.CompressedMessageBufferSize.Min),
                    _loggerFactory.Create<AIMessageDispatcher>(),
                    new AIMessageGroupFactory(_loggerFactory.Create<AIMessageGroup>())),
                _loggerFactory.Create<AIMessageService>());

            return new ServiceContainer(
                AIMessageService: aiMessageService,
                ChatService: chatService,
                MessageService: messageService,
                UserService: userService,
                ConnectionService: connectionService,
                AuthService: authService,
                NotificationService: notificationService
            );
        }
    }
}
