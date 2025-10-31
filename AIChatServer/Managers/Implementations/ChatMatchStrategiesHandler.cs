using AIChatServer.Entities.Chats;
using AIChatServer.Entities.User;
using AIChatServer.Managers.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Managers.Implementations
{
    public class ChatMatchStrategiesHandler(Dictionary<ChatType, IChatMatchStrategy> matchUserStrategies,
        ILogger<ChatMatchStrategiesHandler> logger) : IChatMatchStrategiesHandler
    {

        private readonly Dictionary<ChatType, IChatMatchStrategy> _matchUserStrategies = matchUserStrategies
            ?? throw new ArgumentNullException(nameof(matchUserStrategies));
        private readonly ILogger<ChatMatchStrategiesHandler> _logger = logger
            ?? throw new ArgumentNullException(nameof(logger));

        public bool IsChatSearching(Guid userId)
        {
            foreach (var strategy in _matchUserStrategies.Values)
            {
                if (strategy is IStopableChatMatchStrategy stopable && stopable.IsSearching(userId))
                {
                    _logger.LogDebug("User {UserId} is currently searching in strategy {StrategyType}.", userId, strategy.GetType().Name);
                    return true;
                }
            }
            _logger.LogDebug("User {UserId} is not searching in any strategy.", userId);
            return false;
        }

        public async Task SearchChatAsync(User user, ChatType type, 
            IChatLifecycleManager chatCreator, string? chatMatchPredicate)
        {
            if (_matchUserStrategies.TryGetValue(type, out var strategy))
            {
                _logger.LogInformation("User {UserId} started searching for a chat of type {ChatType}.", user.Id, type);
                await strategy.MatchUserAsync(user, chatCreator, chatMatchPredicate);
                _logger.LogInformation("User {UserId} finished search attempt for chat type {ChatType}.", user.Id, type);
            }
            else
            {
                _logger.LogWarning("No MatchUserStrategy found for chat type {ChatType} when user {UserId} tried to search.", type, user.Id);
            }
        }

        public void StopSearchingChat(Guid userId)
        {
            foreach (var strategy in _matchUserStrategies.Values)
            {
                if (strategy is IStopableChatMatchStrategy stopable)
                {
                    stopable.StopSearching(userId);
                    _logger.LogInformation("User {UserId} stopped searching in strategy {StrategyType}.", userId, strategy.GetType().Name);
                }
            }
        }
    }
}
