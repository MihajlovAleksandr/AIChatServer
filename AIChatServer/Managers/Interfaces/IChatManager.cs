using AIChatServer.Entities.Chats;

namespace AIChatServer.Managers.Interfaces
{
    public interface IChatManager : IChatLifecycleManager, IChatUserManager
    {
        Chat? GetChat(Guid chatId);
        List<Guid> GetUserChats(Guid userId);
    }
}