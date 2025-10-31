using AIChatServer.Entities.Chats;
using AIChatServer.Entities.User;

namespace AIChatServer.Managers.Interfaces
{
    public interface IAddUserStrategiesHandler
    {
        public Task<bool> AddUser(User user, ChatType type, IChatUserManager userAdder, string? chatMatchPredicate);
        public Task<bool> AddChat(User user, IChatUserManager userAdder, Chat chat, string? chatMatchPredicate);
        Guid? IsUserAdding(Guid userId);
        void StopAdding(Guid userId);
    }
}
