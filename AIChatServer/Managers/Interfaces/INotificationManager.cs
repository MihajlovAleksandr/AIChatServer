namespace AIChatServer.Managers.Interfaces
{
    public interface INotificationManager
    {
        Task SendMessageToDevice(string deviceToken, string title, string body, Guid chatId, bool isBodyPrompt);
    }
}
