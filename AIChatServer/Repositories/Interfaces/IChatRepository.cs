using AIChatServer.Entities.Chats;
using AIChatServer.Entities.User;

namespace AIChatServer.Repositories.Interfaces
{
    public interface IChatRepository
    {
        Chat CreateChat(Guid[] users, ChatType type);
        DateTime EndChat(Guid chatId);
        bool UpdateChatName(Guid chatId, Guid userId, string name);
        (List<Chat>, List<Chat>) GetNewChats(Guid userId, DateTime lastOnline);
        (List<Guid>, List<UserData>, List<bool>) LoadUsers(Guid chatId);
        Dictionary<Guid, Chat> GetChats();
        bool AddChatTokenUsage(Guid chatId);
        bool UseToken(Guid chatId, int tokenCount);
    }
}