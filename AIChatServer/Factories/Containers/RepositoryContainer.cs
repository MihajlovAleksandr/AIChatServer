using AIChatServer.Repositories.Interfaces;

namespace AIChatServer.Factories.Containers
{
    public record RepositoryContainer(
        IUserRepository UserRepository,
        IAuthRepository AuthRepository,
        IChatRepository ChatRepository,
        IConnectionRepository ConnectionRepository,
        IAIMessageRepository AIMessageRepository,
        INotificationRepository NotificationRepository
    );
}
