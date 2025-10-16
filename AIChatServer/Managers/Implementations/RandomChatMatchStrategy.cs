using AIChatServer.Entities.Chats;
using AIChatServer.Entities.User;
using AIChatServer.Managers.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Managers.Implementations
{
    public class RandomChatMatchStrategy : IChatMatchStrategy
    {
        private readonly int _probabilityAIChat;
        private readonly Random _random = new();
        private readonly IChatMatchStrategy _aiStrategy;
        private readonly IChatMatchStrategy _humanStrategy;
        private readonly ILogger<RandomChatMatchStrategy> _logger;

        public RandomChatMatchStrategy(int probabilityAIChat, IChatMatchStrategy aiStrategy, IChatMatchStrategy humanStrategy, ILogger<RandomChatMatchStrategy> logger)
        {
            if (probabilityAIChat < 0 || probabilityAIChat > 100)
                throw new ArgumentOutOfRangeException(nameof(probabilityAIChat), "Probability must be between 0 and 100.");

            _probabilityAIChat = probabilityAIChat;
            _aiStrategy = aiStrategy ?? throw new ArgumentNullException(nameof(aiStrategy));
            _humanStrategy = humanStrategy ?? throw new ArgumentNullException(nameof(humanStrategy));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task MatchUserAsync(User user, IChatCreator chatCreator, ChatType? chatType)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (chatCreator == null) throw new ArgumentNullException(nameof(chatCreator));
            double rnd = _random.NextDouble() * 100;

            if (rnd < _probabilityAIChat)
            {
                _logger.LogInformation("User {UserId} assigned to AI chat strategy (rnd={Rnd:F2}).", user.Id, rnd);
                await _aiStrategy.MatchUserAsync(user, chatCreator, ChatType.Random);
            }
            else
            {
                _logger.LogInformation("User {UserId} assigned to Human chat strategy (rnd={Rnd:F2}).", user.Id, rnd);
                await _humanStrategy.MatchUserAsync(user, chatCreator, ChatType.Random);
            }
        }
    }
}
