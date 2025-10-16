using AIChatServer.Entities.Chats;
using AIChatServer.Factories.Interfaces;
using AIChatServer.Managers.Implementations;
using AIChatServer.Managers.Interfaces;

namespace AIChatServer.Factories.Implementations
{
    public class ChatStrategyFactory(ICompositeLoggerFactory loggerFactory) : IChatStrategyFactory
    {
        private readonly ICompositeLoggerFactory _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

        public Dictionary<ChatType, IChatMatchStrategy> CreateStrategies(Guid aIId)
        {
            var humanChatMatchStrategy = new HumanChatMatchStrategy(_loggerFactory.Create<HumanChatMatchStrategy>());
            var aiChatMatchStrategy = new AIChatMatchStrategy(aIId, _loggerFactory.Create<AIChatMatchStrategy>());

            var strategies = new Dictionary<ChatType, IChatMatchStrategy>
            {
                { ChatType.Human, humanChatMatchStrategy },
                { ChatType.AI, aiChatMatchStrategy },
                { ChatType.Random, new RandomChatMatchStrategy(100, aiChatMatchStrategy, humanChatMatchStrategy,
                _loggerFactory.Create<RandomChatMatchStrategy>()) }
            };

            return strategies;
        }
    }
}
