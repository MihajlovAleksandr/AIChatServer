using AIChatServer.Entities.Chats;
using AIChatServer.Managers.Interfaces;

namespace AIChatServer.Factories.Containers
{
    public record ChatStrategyContainer(
        Dictionary<ChatType, IChatMatchStrategy> ChatMatchStrategies,
        Dictionary<ChatType, IAddUserStrategy> AddUserStrategies
    );
}
