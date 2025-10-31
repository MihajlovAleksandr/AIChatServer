using AIChatServer.Entities.Chats;
using AIChatServer.Entities.User;

namespace AIChatServer.Managers.Interfaces
{
    public interface IChatMatchStrategiesHandler
    {
        Task SearchChatAsync(User user, ChatType chatType, IChatLifecycleManager chatCreator, string? chatMatchPredicate);
        bool IsChatSearching(Guid userId);
        void StopSearchingChat(Guid userId);
    }
}
