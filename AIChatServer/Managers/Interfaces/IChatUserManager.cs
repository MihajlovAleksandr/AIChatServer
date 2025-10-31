using AIChatServer.Entities.Chats;
using AIChatServer.Entities.User;

namespace AIChatServer.Managers.Interfaces
{
    public interface IChatUserManager
    {
        event Action<User, Chat> OnUserAdded;
        event Action<Guid, Chat> OnUserRemoved;
        void AddUserToChat(User user, Guid chatId);
        Guid[] GetUsersInChat(Guid chatId);
        void RemoveUserFromChat(Guid userId, Guid chatId);
    }
}
