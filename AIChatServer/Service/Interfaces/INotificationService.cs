namespace AIChatServer.Service.Interfaces
{
    public interface INotificationService
    {
        bool UpdateNotifications(Guid userId, bool value);
        bool GetNotifications(Guid userId);
        bool UpdateNotificationToken(Guid id, string token);
        Dictionary<Guid, List<string>> GetNotificationTokens(Guid[] userIds);
        List<string> GetNotificationTokens(Guid userId);
    }
}