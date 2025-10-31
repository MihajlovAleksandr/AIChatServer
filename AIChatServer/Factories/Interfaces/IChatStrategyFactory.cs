using AIChatServer.Config.Data;
using AIChatServer.Factories.Containers;

namespace AIChatServer.Factories.Interfaces
{
    public interface IChatStrategyFactory
    {
        ChatStrategyContainer CreateStrategies(AIConfigData aIConfigData, UserPredicateContainer userPredicateContainer);
    }
}
