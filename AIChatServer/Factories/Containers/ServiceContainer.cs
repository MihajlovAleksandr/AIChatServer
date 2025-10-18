using AIChatServer.Service.Interfaces;

namespace AIChatServer.Factories.Containers
{
    public record ServiceContainer(
        IAIMessageService AIMessageService,
        IChatService ChatService,
        IMessageService MessageService, 
        IUserService UserService,
        IConnectionService ConnectionService,
        IAuthService AuthService,
        INotificationService NotificationService
    );
}
