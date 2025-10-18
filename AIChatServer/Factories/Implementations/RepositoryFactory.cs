using AIChatServer.Config.Data;
using AIChatServer.Factories.Containers;
using AIChatServer.Factories.Interfaces;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Repositories.Implementations;

namespace AIChatServer.Factories.Implementations
{
    public class RepositoryFactory(ICompositeLoggerFactory loggerFactory, string connectionString) : IRepositoryFactory
    {
        private readonly ICompositeLoggerFactory _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        private readonly string _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

        public RepositoryContainer CreateRepositories(ConfigData configData)
        {
            var userRepository = new UserRepository(_connectionString, _loggerFactory.Create<UserRepository>());
            var authRepository = new AuthRepository(userRepository, _connectionString, _loggerFactory.Create<AuthRepository>());
            var chatRepository = new ChatRepository(_connectionString, _loggerFactory.Create<ChatRepository>());
            var messageRepository = new MessageRepository(_connectionString, _loggerFactory.Create<MessageRepository>());
            var connectionRepository = new ConnectionRepository(_connectionString, authRepository, _loggerFactory.Create<ConnectionRepository>());
            var aiMessageRepository = new AIMessageRepository(_connectionString, _loggerFactory.Create<AIMessageRepository>());
            var notificationRepository = new NotificationRepository(_connectionString, _loggerFactory.Create<NotificationRepository>());

            return new RepositoryContainer(userRepository, authRepository, chatRepository, messageRepository, connectionRepository,
                aiMessageRepository, notificationRepository);
        }
    }
}
