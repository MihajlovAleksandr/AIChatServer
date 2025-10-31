using AIChatServer.Entities.Chats;

namespace AIChatServer.Managers.Interfaces
{
    public interface IChatLifecycleManager
    {
        event Action<Chat> OnChatCreated; 
        event Action<Chat> OnChatEnded;

        void CreateChat(Guid[] users, ChatType type);
        void EndChat(Guid chatId);
    }
}
