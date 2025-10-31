using AIChatServer.Entities.Chats;
using AIChatServer.Entities.User;
using AIChatServer.Managers.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Managers.Implementations
{
    public class AddUserStrategiesHandler(Dictionary<ChatType, IAddUserStrategy> addUserStrategies,
        ILogger<AddUserStrategiesHandler> logger) : IAddUserStrategiesHandler
    {

        private readonly Dictionary<ChatType, IAddUserStrategy> _addUserStrategies = addUserStrategies
            ?? throw new ArgumentNullException(nameof(addUserStrategies));
        private readonly ILogger<AddUserStrategiesHandler> _logger = logger
            ?? throw new ArgumentNullException(nameof(logger));

        public async Task<bool> AddUser(User user, ChatType type, IChatUserManager userAdder, string? chatMatchPredicate)
        {
            if (_addUserStrategies.TryGetValue(type, out var strategy))
            {
                _logger.LogInformation("User {UserId} started adding for a chat of type {chatType}", user.Id, type);
                bool isAdded = await strategy.AddUser(user, userAdder, chatMatchPredicate);
                _logger.LogInformation("User {UserId} finished adding attempt for chat type {ChatType}.", user.Id, type);
                return isAdded;
            }
            else
            {
                _logger.LogWarning("No AddUserStrategy found for chat type {ChatType} when user {UserId} tried to search.", type, user.Id);
            }
            return false;
        }

        public async Task<bool> AddChat(User user, IChatUserManager userAdder, Chat chat, string? chatMatchPredicate)
        {
            if (_addUserStrategies.TryGetValue(chat.Type, out var strategy))
            {
                _logger.LogInformation("User {UserId} started adding for a chat {chatId}", user.Id, chat.Id);
                 bool isAdded = await strategy.AddChat(user, userAdder, chat.Id, chatMatchPredicate);
                _logger.LogInformation("User {UserId} finished adding attempt for chat {chatId}.", user.Id, chat.Id);
                return isAdded;
            }
            else
            {
                _logger.LogWarning("No AddUserStrategy found for chat type {ChatType} when user {UserId} tried to search.",
                    chat.Type, user.Id);
                throw new KeyNotFoundException();
            }
        }

        public Guid? IsUserAdding(Guid userId)
        {
            foreach (var strategy in _addUserStrategies.Values)
            {
                if (strategy is IStopableAddUserStrategy stopable)
                {
                    Guid? chatId = stopable.IsSearching(userId);
                    if (chatId != null) return chatId;
                    _logger.LogDebug("User {UserId} is currently searching in strategy {StrategyType}.", userId, strategy.GetType().Name);
                    return chatId;
                }
            }
            _logger.LogDebug("User {UserId} is not searching in any strategy.", userId);
            return null;
        }

        public void StopAdding(Guid userId)
        {
            foreach (var strategy in _addUserStrategies.Values)
            {
                if (strategy is IStopableAddUserStrategy stopable)
                {
                    stopable.StopSearching(userId);
                    _logger.LogInformation("User {UserId} stopped searching in strategy {StrategyType}.", userId, strategy.GetType().Name);
                }
            }
        }
    }
}
