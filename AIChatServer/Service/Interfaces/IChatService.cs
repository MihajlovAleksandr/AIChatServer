using AIChatServer.Entities.Chats;
using AIChatServer.Entities.User;

namespace AIChatServer.Service.Interfaces
{
    public interface IChatService
    {
        Chat CreateChat(Guid[] users, ChatType type);
        List<Message> GetMessagesByChatId(Guid chatId);
        Message SendMessage(Message message);
        DateTime EndChat(Guid chatId);
        bool UpdateChatName(Guid chatId, Guid userId, string name);
        (List<Chat>, List<Chat>) GetNewChats(Guid userId, DateTime lastOnline);
        (List<Message>, List<Message>) GetNewMessages(Guid userId, DateTime lastOnline);
        (List<Guid>, List<UserData>, List<bool>) LoadUsers(Guid chatId);
        Dictionary<Guid, Chat> GetChats();
        bool AddChatTokenUsage(Guid chatId);
        bool UseToken(Guid chatId, int tokenCount);
    }
}