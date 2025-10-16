using AIChatServer.Entities.Chats;
using AIChatServer.Entities.User;

namespace AIChatServer.Managers.Interfaces
{
    public interface IChatMatchStrategy
    {
        Task MatchUserAsync(User user, IChatCreator chatCreator, ChatType? chatType = default);
    }
}
