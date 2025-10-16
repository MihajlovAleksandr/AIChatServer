namespace AIChatServer.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        bool UpdateNotifications(Guid userId, bool value);
        bool GetNotifications(Guid userId);
        bool UpdateNotificationToken(Guid id, string token);
        Dictionary<Guid, List<string>> GetNotificationTokens(Guid[] userIds);
    }
}