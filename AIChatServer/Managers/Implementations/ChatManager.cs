using AIChatServer.Entities.Chats;
using AIChatServer.Entities.User;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Managers.Implementations
{
    public class ChatManager : IChatManager, IChatCreator
    {
        private readonly IChatService _chatService;
        private readonly Dictionary<Guid, Chat> _chatList;
        private readonly Dictionary<ChatType, IChatMatchStrategy> _strategies;
        private readonly ILogger<ChatManager> _logger;
        private readonly object _syncObj = new();

        public event Action<Chat> OnChatCreated;
        public event Action<Chat> OnChatEnded;

        public ChatManager(
            IChatService chatService, 
            Dictionary<ChatType, IChatMatchStrategy> strategies,
            ILogger<ChatManager> logger)
        {
            _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService)); ;
            _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _chatList = chatService.GetChats();
            _logger.LogInformation("ChatManager initialized with {ChatCount} existing chats.", _chatList.Count);
        }

        public async Task SearchChatAsync(User user, ChatType type)
        {
            if (_strategies.TryGetValue(type, out var strategy))
            {
                _logger.LogInformation("User {UserId} started searching for a chat of type {ChatType}.", user.Id, type);
                await strategy.MatchUserAsync(user, this);
                _logger.LogInformation("User {UserId} finished search attempt for chat type {ChatType}.", user.Id, type);
            }
            else
            {
                _logger.LogWarning("No strategy found for chat type {ChatType} when user {UserId} tried to search.", type, user.Id);
            }
        }

        public void StopSearchingChat(Guid userId)
        {
            foreach (var strategy in _strategies.Values)
            {
                if (strategy is IStopableChatMatchStrategy stopable)
                {
                    stopable.StopSearching(userId);
                    _logger.LogInformation("User {UserId} stopped searching in strategy {StrategyType}.", userId, strategy.GetType().Name);
                }
            }
        }

        public bool IsSearchingChat(Guid userId)
        {
            foreach (var strategy in _strategies.Values)
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

        public void EndChat(Guid chatId)
        {
            lock (_syncObj)
            {
                if (_chatList.TryGetValue(chatId, out var chat))
                {
                    chat.EndTime = _chatService.EndChat(chatId);
                    OnChatEnded?.Invoke(chat);
                    _chatList.Remove(chatId);
                    _logger.LogInformation("Chat {ChatId} ended.", chatId);
                }
                else
                {
                    _logger.LogWarning("Attempted to end non-existing chat {ChatId}.", chatId);
                }
            }
        }

        public List<Guid> GetUsersInChat(Guid chatId)
        {
            lock (_syncObj)
            {
                if (_chatList.TryGetValue(chatId, out var chat))
                    return chat.UsersNames.Keys.ToList();
                _logger.LogWarning("Requested users for non-existing chat {ChatId}.", chatId);
                return new List<Guid>();
            }
        }

        public List<Guid> GetUserChats(Guid userId)
        {
            var chats = _chatList.Values
                           .Where(c => c.UsersNames.ContainsKey(userId))
                           .Select(c => c.Id)
                           .ToList();
            _logger.LogDebug("User {UserId} is in {ChatCount} chats.", userId, chats.Count);
            return chats;
        }

        public ChatType? GetChatType(Guid chatId)
        {
            ChatType? type = _chatList.TryGetValue(chatId, out var chat) ? chat.Type : null;
            _logger.LogDebug("ChatType requested for chat {ChatId}: {ChatType}", chatId, type);
            return type;
        }

        public string? GetChatName(Guid chatId, Guid userId)
        {
            var name = _chatList.TryGetValue(chatId, out var chat) && chat.UsersNames.TryGetValue(userId, out var userName)
                ? userName
                : null;
            _logger.LogDebug("Chat name requested for chat {ChatId}, user {UserId}: {ChatName}", chatId, userId, name);
            return name;
        }

        public void CreateChat(Guid[] users, ChatType type)
        {
            var chat = _chatService.CreateChat(users, type);
            lock (_syncObj)
            {
                _chatList.Add(chat.Id, chat);
            }
            _logger.LogInformation("Created new chat {ChatId} of type {ChatType} with users {UserIds}.", chat.Id, type, string.Join(",", users));
            OnChatCreated?.Invoke(chat);
        }
    }
}
