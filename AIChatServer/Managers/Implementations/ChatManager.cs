using AIChatServer.Entities.Chats;
using AIChatServer.Entities.User;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Service.Interfaces;
using Microsoft.Extensions.Logging;
using System;

namespace AIChatServer.Managers.Implementations
{
    public class ChatManager : IChatManager
    {
        private readonly IChatService _chatService;
        private readonly Dictionary<Guid, Chat> _chatList;
        private readonly ILogger<ChatManager> _logger;
        private readonly object _syncObj = new();

        public event Action<Chat> OnChatCreated;
        public event Action<Chat> OnChatEnded;
        public event Action<User, Chat> OnUserAdded;
        public event Action<Guid, Chat> OnUserRemoved;

        public ChatManager(
            IChatService chatService, 
            Dictionary<ChatType, IChatMatchStrategy> strategies,
            ILogger<ChatManager> logger)
        {
            _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _chatList = chatService.GetChats();
            _logger.LogInformation("ChatManager initialized with {ChatCount} existing chats.", _chatList.Count);
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

        public List<Guid> GetUserChats(Guid userId)
        {
            var chats = _chatList.Values
                           .Where(c => c.UsersWithData.ContainsKey(userId))
                           .Select(c => c.Id)
                           .ToList();
            _logger.LogDebug("User {UserId} is in {ChatCount} chats.", userId, chats.Count);
            return chats;
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

        public void AddUserToChat(User user, Guid chatId)
        {
            UserInChatData chatName = _chatService.AddUserToChat(user.Id, chatId);
            lock (_syncObj)
            {
                Chat? chat = GetChat(chatId);
                if (chat == null)
                {
                    _logger.LogError("User was not added to chat: Chat {chat} was not in chatList", chatId);
                    return;
                }
                chat.UsersWithData.Add(user.Id, chatName);
                _logger.LogInformation("Add user {userId} to chat {chatId} with name = {chatName}.", user.Id, chatId, chatName);
                OnUserAdded?.Invoke(user, chat);
            }
        }

        public Chat? GetChat(Guid chatId)
        {
            _chatList.TryGetValue(chatId, out Chat? chat);
            return chat;
        }

        public void RemoveUserFromChat(Guid userId, Guid chatId)
        {
            Chat? chat = GetChat(chatId);
            if (chat == null)
            {
                _logger.LogError("User was not removed from chat: Chat {chat} was not in chatList", chatId);
                return;
            }
            _chatService.RemoveUserFromChat(userId, chatId);
            if(!chat.UsersWithData.Remove(userId))
            {
                _logger.LogError("User was not removed from chat {chatId}: User {user} was not in chat.UserNames", chatId, userId);
                return;
            }
            OnUserRemoved?.Invoke(userId, chat);
        }

        public Guid[] GetUsersInChat(Guid chatId)
        {
            Chat chat = GetChat(chatId) 
                ?? throw new ArgumentNullException(nameof(chat));
            return chat.UsersWithData.Keys.ToArray();
        }
    }
}
