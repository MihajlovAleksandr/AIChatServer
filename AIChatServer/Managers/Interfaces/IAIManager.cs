using AIChatServer.Entities.Chats;

namespace AIChatServer.Managers.Interfaces
{
    public interface IAIManager
    {
        Guid AIId { get; }
        event Action<Message> OnSendMessage;
        void CreateDialog(Guid chatId);
        void EndDialog(Guid chatId);
        Task SendMessageAsync(Guid chatId, string message);
    }
}
