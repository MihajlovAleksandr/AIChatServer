using AIChatServer.Managers.Interfaces;

namespace AIChatServer.Factories.Containers
{
    public record ManagerContainer
    (
        IChatManager ChatManager,
        IUserManager UserManager,
        INotificationManager NotificationManager,
        ISyncManager SyncService,
        IAIManager AIManager,
        IUserEvents UserEvents,
        IConnectionListener ConnectionListener
    );
}
