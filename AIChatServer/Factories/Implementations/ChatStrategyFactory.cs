using AIChatServer.Config.Data;
using AIChatServer.Entities.Chats;
using AIChatServer.Factories.Containers;
using AIChatServer.Factories.Interfaces;
using AIChatServer.Managers.Implementations.ChatStrategies.AddUserOrChatStrategies;
using AIChatServer.Managers.Implementations.ChatStrategies.MatchStrategies;
using AIChatServer.Managers.Interfaces;

namespace AIChatServer.Factories.Implementations
{
    public class ChatStrategyFactory(ICompositeLoggerFactory loggerFactory) : IChatStrategyFactory
    {
        private readonly ICompositeLoggerFactory _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

        public ChatStrategyContainer CreateStrategies(AIConfigData aIConfigData,
            UserPredicateContainer userPredicateContainer)
        {
            var humanChatMatchStrategy = new HumanChatMatchStrategy(userPredicateContainer.Predicates,
                userPredicateContainer.DefaultPredicate, _loggerFactory.Create<HumanChatMatchStrategy>());
            var aiChatMatchStrategy = new AIChatMatchStrategy(aIConfigData.AIId, _loggerFactory.Create<AIChatMatchStrategy>());
            var randomChatMatchStrategy = new RandomChatMatchStrategy(aIConfigData.ProbabilityAIChat,
                aIConfigData.AIId, (aIConfigData.AIChatDelay.Min, aIConfigData.AIChatDelay.Max),
                _loggerFactory.Create<RandomChatMatchStrategy>());
            var groupStrategy = new GroupChatMatchStrategy(_loggerFactory.Create<GroupChatMatchStrategy>());

            var groupUserAdder = new AddUserGroupStrategy(userPredicateContainer.Predicates,
                userPredicateContainer.DefaultPredicate, loggerFactory.Create<AddUserGroupStrategy>());

            var chatMatchstrategies = new Dictionary<ChatType, IChatMatchStrategy>
            {
                { ChatType.Human, humanChatMatchStrategy },
                { ChatType.AI, aiChatMatchStrategy },
                { ChatType.Random, randomChatMatchStrategy},
                { ChatType.Group, groupStrategy }
            };


            var userAddStrategy = new Dictionary<ChatType, IAddUserStrategy>
            {
                {ChatType.Group,  groupUserAdder}
            };

            return new ChatStrategyContainer(chatMatchstrategies, userAddStrategy);
        }
    }
}
