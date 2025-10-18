using AIChatServer.Entities.Chats;

namespace AIChatServer.Repositories.Interfaces
{
    public interface IMessageRepository
    {
        Message SendMessage(Guid id, Guid chatId, Guid userId, string text); 
        List<Message> GetMessagesByChatId(Guid chatId);
        (List<Message>, List<Message>) GetNewMessages(Guid userId, DateTime lastOnline);
        Message? GetMessageById(Guid id);
    }
}
