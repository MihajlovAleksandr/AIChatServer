using AIChatServer.Entities.Chats;
using AIChatServer.Factories.Containers;
using AIChatServer.Managers.Interfaces;

namespace AIChatServer.Factories.Interfaces
{
    public interface IChatControllerFactory
    {
        ChatControllerContainer Create(Dictionary<ChatType, IChatMatchStrategy> chatMatchStrategies,
            Dictionary<ChatType, IAddUserStrategy> addUserStrategies);
    }
}
