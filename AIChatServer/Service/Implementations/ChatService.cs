using AIChatServer.Entities.Chats;
using AIChatServer.Entities.User;
using AIChatServer.Repositories.Interfaces;
using AIChatServer.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Service.Implementations
{
    public class ChatService(IChatRepository chatRepository, ILogger<ChatService> logger) : IChatService
    {
        private readonly IChatRepository _chatRepository = chatRepository 
            ?? throw new ArgumentNullException(nameof(chatRepository));
        private readonly ILogger<ChatService> _logger = logger 
            ?? throw new ArgumentNullException(nameof(logger));

        public Chat CreateChat(Guid[] users, ChatType type)
        {
            ArgumentNullException.ThrowIfNull(users);

            if (users.Length == 0)
            {
                _logger.LogWarning("Attempt to create chat with empty users array.");
                throw new ArgumentException("Users array cannot be empty", nameof(users));
            }

            if (users.Distinct().Count() != users.Length)
            {
                _logger.LogWarning("Attempt to create chat with duplicate users. Users: {Users}", string.Join(",", users));
                throw new ArgumentException("Users array contains duplicates", nameof(users));
            }

            var chat = _chatRepository.CreateChat(users, type);
            _logger.LogInformation("Chat {ChatId} created with {UserCount} users of type {ChatType}.", chat.Id, users.Length, type);

            return chat;
        }

        public DateTime EndChat(Guid chatId)
        {
            var endTime = _chatRepository.EndChat(chatId);
            _logger.LogInformation("Chat {ChatId} ended at {EndTime}.", chatId, endTime);
            return endTime;
        }

        public bool UpdateChatName(Guid chatId, Guid userId, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("User {UserId} tried to set empty chat name for Chat {ChatId}.", userId, chatId);
                throw new ArgumentException("Name cannot be null or whitespace", nameof(name));
            }

            var success = _chatRepository.UpdateChatName(chatId, userId, name);
            if (success)
            {
                _logger.LogInformation("Chat {ChatId} name updated to \"{ChatName}\" by User {UserId}.", chatId, name, userId);
            }
            else
            {
                _logger.LogWarning("Failed to update Chat {ChatId} name by User {UserId}.", chatId, userId);
            }

            return success;
        }

        public (List<Chat>, List<Chat>) GetNewChats(Guid userId, DateTime lastOnline)
        {
            var chats = _chatRepository.GetNewChats(userId, lastOnline);
            _logger.LogInformation("Retrieved {NewChats} new and {UpdatedChats} updated chats for User {UserId}.", chats.Item1.Count, chats.Item2.Count, userId);
            return chats;
        }

        public (List<Guid>, List<UserData>, List<bool>) LoadUsers(Guid chatId)
        {
            var users = _chatRepository.LoadUsers(chatId);
            _logger.LogInformation("Loaded {UserCount} users for Chat {ChatId}.", users.Item1.Count, chatId);
            return users;
        }

        public Dictionary<Guid, Chat> GetChats()
        {
            var chats = _chatRepository.GetChats();
            _logger.LogInformation("Retrieved all chats. Total count: {ChatCount}.", chats.Count);
            return chats;
        }

        public bool AddChatTokenUsage(Guid chatId)
        {
            var success = _chatRepository.AddChatTokenUsage(chatId);
            if (success)
                _logger.LogInformation("Token usage incremented for Chat {ChatId}.", chatId);
            else
                _logger.LogWarning("Failed to increment token usage for Chat {ChatId}.", chatId);

            return success;
        }

        public bool UseToken(Guid chatId, int tokenCount)
        {
            if (tokenCount < 0)
            {
                _logger.LogWarning("Attempt to use negative token count {TokenCount} for Chat {ChatId}.", tokenCount, chatId);
                throw new ArgumentException("Token count cannot be negative", nameof(tokenCount));
            }

            var success = _chatRepository.UseToken(chatId, tokenCount);
            if (success)
                _logger.LogInformation("{TokenCount} tokens used in Chat {ChatId}.", tokenCount, chatId);
            else
                _logger.LogWarning("Failed to use {TokenCount} tokens in Chat {ChatId}.", tokenCount, chatId);

            return success;
        }
    }
}
