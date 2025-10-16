using AIChatServer.Entities.AI;
using AIChatServer.Entities.AI.Implementations;
using AIChatServer.Entities.AI.Interfaces;
using AIChatServer.Repositories.Interfaces;
using AIChatServer.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Service.Implementations
{
    public class AIMessageService(
        IAIMessageRepository aiMessageRepository,
        IAIMessageDispatcherFactory aIMessageDispatcherFactory,
        ILogger<AIMessageService> logger) : IAIMessageService
    {
        private readonly IAIMessageRepository _aiMessageRepository = aiMessageRepository 
            ?? throw new ArgumentNullException(nameof(aiMessageRepository));
        private readonly ILogger<AIMessageService> _logger = logger 
            ?? throw new ArgumentNullException(nameof(logger));
        private readonly IAIMessageDispatcherFactory _aIMessageDispatcherFactory = aIMessageDispatcherFactory 
            ?? throw new ArgumentNullException(nameof(aIMessageDispatcherFactory));

        public AIMessage AddAIMessage(AIMessage aiMessage, string type)
        {
            ArgumentNullException.ThrowIfNull(aiMessage);

            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Type cannot be null or empty", nameof(type));

            try
            {
                _logger.LogInformation("Adding AI message for Chat {ChatId} with Role {Role} and Type {Type}.",
                    aiMessage.ChatId, aiMessage.Role, type);

                var addedMessage = _aiMessageRepository.AddAIMessage(aiMessage.ChatId, aiMessage.Content, aiMessage.Role, type);

                _logger.LogInformation("AI message {MessageId} successfully added to Chat {ChatId}.",
                    addedMessage.Id, aiMessage.ChatId);

                return addedMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding AI message for Chat {ChatId}.", aiMessage.ChatId);
                throw;
            }
        }

        public async Task<Dictionary<Guid, AIMessageDispatcher>> GetAIMessagesByChatAsync(List<Guid> chatIds)
        {
            ArgumentNullException.ThrowIfNull(chatIds);

            _logger.LogInformation("Fetching AI messages for {Count} chats.", chatIds.Count);

            var aiMessageDispatchers = new Dictionary<Guid, AIMessageDispatcher>();

            try
            {
                foreach (var aiMessages in _aiMessageRepository.GetAIMessagesByChat(chatIds))
                {
                    var dispatcher = _aIMessageDispatcherFactory.Create();
                    await dispatcher.SetMainMessagesAsync(aiMessages.Value.Item1);
                    await dispatcher.SetCompressedMessagesAsync(aiMessages.Value.Item2);

                    aiMessageDispatchers.Add(aiMessages.Key, dispatcher);

                    _logger.LogInformation("Loaded {MainCount} main and {CompressedCount} compressed messages for Chat {ChatId}.",
                        aiMessages.Value.Item1.Count, aiMessages.Value.Item2.Count, aiMessages.Key);
                }

                return aiMessageDispatchers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching AI messages for chats.");
                throw;
            }
        }

        public async Task<AIMessageDispatcher> GetAIMessagesByChatAsync(Guid chatId)
        {
            _logger.LogInformation("Fetching AI messages for Chat {ChatId}.", chatId);

            var dispatcher = _aIMessageDispatcherFactory.Create();

            try
            {
                var result = _aiMessageRepository.GetAIMessagesByChat(new List<Guid> { chatId });
                if (result.TryGetValue(chatId, out var messages))
                {
                    await dispatcher.SetMainMessagesAsync(messages.Item1);
                    await dispatcher.SetCompressedMessagesAsync(messages.Item2);

                    _logger.LogInformation("Loaded {MainCount} main and {CompressedCount} compressed messages for Chat {ChatId}.",
                        messages.Item1.Count, messages.Item2.Count, chatId);
                }
                else
                {
                    _logger.LogInformation("No messages found for Chat {ChatId}.", chatId);
                }

                return dispatcher;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching AI messages for Chat {ChatId}.", chatId);
                throw;
            }
        }

        public bool DeleteAIMessages(List<AIMessage> messages)
        {
            ArgumentNullException.ThrowIfNull(messages);

            if (messages.Count == 0)
            {
                _logger.LogInformation("No AI messages provided for deletion. Skipping.");
                return true;
            }

            try
            {
                _logger.LogInformation("Deleting {Count} AI messages.", messages.Count);

                var result = _aiMessageRepository.DeleteAIMessages(messages.Select(m => m.Id).ToArray());

                if (result)
                    _logger.LogInformation("Successfully deleted {Count} AI messages.", messages.Count);
                else
                    _logger.LogWarning("Failed to delete some AI messages.");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting AI messages.");
                throw;
            }
        }

        public bool DeleteAIMessage(AIMessage message)
        {
            ArgumentNullException.ThrowIfNull(message);

            _logger.LogInformation("Deleting single AI message {MessageId} for Chat {ChatId}.", message.Id, message.ChatId);

            return DeleteAIMessages(new List<AIMessage> { message });
        }
    }
}
