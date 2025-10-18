using AIChatServer.Entities.Chats;

namespace AIChatServer.Service.Interfaces
{
    public interface IMessageService
    {
        List<Message> GetMessagesByChatId(Guid chatId);
        Message SendMessage(Message message);
        (List<Message>, List<Message>) GetNewMessages(Guid userId, DateTime lastOnline);
        Message? GetMessageById(Guid id);
    }
}
