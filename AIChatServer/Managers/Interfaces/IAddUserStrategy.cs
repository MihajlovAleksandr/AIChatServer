using AIChatServer.Entities.User;

namespace AIChatServer.Managers.Interfaces
{
    public interface IAddUserStrategy
    {
        Task<bool> AddUser(User user, IChatUserManager userAdder, string? predicate = null);
        Task<bool> AddChat(User user, IChatUserManager userAdder, Guid chatId, string? predicate = null);
    }
}
