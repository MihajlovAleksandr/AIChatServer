using AIChatServer.Entities.Chats;

namespace AIChatServer.Managers.Interfaces
{
    public interface IChatCreator
    {
        void CreateChat(Guid[] users, ChatType type);
    }
}
