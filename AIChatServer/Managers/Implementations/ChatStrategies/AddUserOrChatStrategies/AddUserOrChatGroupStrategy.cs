using AIChatServer.Entities.User;
using AIChatServer.Managers.Implementations.ChatMatchStrategies;
using AIChatServer.Managers.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Managers.Implementations.ChatStrategies.AddUserOrChatStrategies
{
    public class AddUserGroupStrategy : ChatPredicateHandler, IStopableAddUserStrategy
    {
        private readonly List<(User user, IUserMatchPredicate predicate)> _waitingUsers = new();
        private readonly List<(User user, IUserMatchPredicate predicate, Guid chatId)> _waitingChats = new();
        private readonly ILogger<AddUserGroupStrategy> _logger;

        public AddUserGroupStrategy(
            Dictionary<string, IUserMatchPredicate> predicates,
            IUserMatchPredicate defaultPredicate,
            ILogger<AddUserGroupStrategy> logger)
            : base(predicates, defaultPredicate)
        {
            _logger = logger;
            _logger.LogInformation("GroupChatMatchStrategy initialized with {PredicateCount} predicates", predicates.Count);
        }

        public Guid? IsSearching(Guid userId)
        {
            if (_waitingUsers.FirstOrDefault(waitingUser =>
                waitingUser.user.Id == userId) != default) return Guid.Empty;
            
            (User user, IUserMatchPredicate predicate, Guid chatId)? waitingChat = _waitingChats.FirstOrDefault(waitingChat =>
               waitingChat.user.Id == userId);
            if (waitingChat != default)
            {
                return waitingChat.Value.chatId;
            }

            return default;
        }

        public void StopSearching(Guid userId)
        {
            int usersRemoved = _waitingUsers.RemoveAll(waitingUser =>
                waitingUser.user.Id == userId);
            int chatsRemoved = _waitingChats.RemoveAll(waitingChat =>
                waitingChat.user.Id == userId);

            _logger.LogInformation("Stopped searching for user {UserId}. Removed {UsersRemoved} from waiting users and {ChatsRemoved} from waiting chats",
                userId, usersRemoved, chatsRemoved);
        }

        public async Task<bool> AddChat(User user, IChatUserManager userAdder, Guid chatId, string? predicate)
        {
            _logger.LogDebug("Attempting to match chat {ChatId} for user {UserId}", chatId, user.Id);

            IUserMatchPredicate userMatchPredicate = GetPredicate(predicate);

            for (int i = 0; i < _waitingUsers.Count; i++)
            {
                var waitingUser = _waitingUsers[i];
                if (userAdder.GetUsersInChat(chatId).Contains(waitingUser.user.Id)) continue;
                _logger.LogTrace("Checking match between chat user {UserId} and waiting user {WaitingUserId}",
                    user.Id, waitingUser.user.Id);

                if (userMatchPredicate.TryMatch(user, waitingUser.user)
                    && waitingUser.predicate.TryMatch(waitingUser.user, user))
                {
                    _logger.LogInformation("Match found! Adding user {UserId} to chat {ChatId}", user.Id, chatId);

                    try
                    {
                        userAdder.AddUserToChat(waitingUser.user, chatId);
                        _waitingUsers.RemoveAt(i);
                        _logger.LogInformation("Successfully added user {UserId} to chat {ChatId} and removed from waiting list",
                            waitingUser.user.Id, chatId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to add user {UserId} to chat {ChatId}", waitingUser.user.Id, chatId);
                        throw;
                    }
                    return true;
                }
            }

            _waitingChats.Add((user, userMatchPredicate, chatId));
            _logger.LogInformation("No match found for chat {ChatId}. User {UserId} added to waiting chats. Total waiting chats: {WaitingChatsCount}",
                chatId, user.Id, _waitingChats.Count);
            return await Task.FromResult(false);
        }

        public async Task<bool> AddUser(User user, IChatUserManager userAdder, string? predicate)
        {
            _logger.LogDebug("Attempting to match user {UserId} with existing chats", user.Id);

            IUserMatchPredicate userMatchPredicate = GetPredicate(predicate);

            for (int i = 0; i < _waitingChats.Count; i++)
            {
                var waitingChat = _waitingChats[i];
                if (userAdder.GetUsersInChat(waitingChat.chatId).Contains(user.Id)) continue;
                _logger.LogTrace("Checking match between user {UserId} and waiting chat {ChatId}",
                    user.Id, waitingChat.chatId);

                if (userMatchPredicate.TryMatch(user, waitingChat.user)
                    && waitingChat.predicate.TryMatch(waitingChat.user, user))
                {
                    _logger.LogInformation("Match found! Adding user {UserId} to chat {ChatId}", user.Id, waitingChat.chatId);

                    try
                    {
                        userAdder.AddUserToChat(user, waitingChat.chatId);
                        _waitingChats.RemoveAt(i);
                        _logger.LogInformation("Successfully added user {UserId} to chat {ChatId} and removed chat from waiting list",
                            user.Id, waitingChat.chatId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to add user {UserId} to chat {ChatId}", user.Id, waitingChat.chatId);
                        throw;
                    }
                    return true;
                }
            }

            _waitingUsers.Add((user, userMatchPredicate));
            _logger.LogInformation("No match found for user {UserId}. Added to waiting users. Total waiting users: {WaitingUsersCount}",
                user.Id, _waitingUsers.Count);
            return await Task.FromResult(false);
        }
    }
}
