using AIChatServer.Entities.Chats;
using AIChatServer.Entities.User;

namespace AIChatServer.Managers.Interfaces
{
    public interface IChatMatchStrategy
    {
        Task MatchUserAsync(User user, IChatLifecycleManager chatCreator, string? chatPredicate = null);
    }
}
