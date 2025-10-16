using AIChatServer.Entities.Chats;
using AIChatServer.Entities.User;

namespace AIChatServer.Managers.Interfaces
{
    public interface IChatManager
    {
        event Action<Chat> OnChatCreated;
        event Action<Chat> OnChatEnded;

        List<Guid> GetUsersInChat(Guid chatId);
        void EndChat(Guid chatId);
        Task SearchChatAsync(User user, ChatType type);
        void StopSearchingChat(Guid userId);
        List<Guid> GetUserChats(Guid userId);
        ChatType? GetChatType(Guid chatId);
        string? GetChatName(Guid chatId, Guid userId);
        bool IsSearchingChat(Guid userId);
    }
}
