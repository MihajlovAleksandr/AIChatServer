using AIChatServer.Entities.Chats;
using AIChatServer.Managers.Interfaces;

namespace AIChatServer.Factories.Interfaces
{
    public interface IChatStrategyFactory
    {
        Dictionary<ChatType, IChatMatchStrategy> CreateStrategies(Guid aIId);
    }
}
