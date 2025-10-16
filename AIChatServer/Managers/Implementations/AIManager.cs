using AIChatServer.Entities.AI;
using AIChatServer.Entities.AI.Implementations;
using AIChatServer.Entities.AI.Interfaces;
using AIChatServer.Entities.Chats;
using AIChatServer.Integrations.AI.DTO;
using AIChatServer.Integrations.AI.Interfaces;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Service.Interfaces;
using AIChatServer.Utils.Interfaces.Mapper;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Managers.Implementations
{
    public class AIManager : IAIManager
    {
        private readonly IAIController _aiController;
        private readonly Dictionary<Guid, AIMessageDispatcher> _dialogs;
        private readonly IChatService _chatService;
        private readonly IAIMessageService _aiMessageService;
        private readonly IResponseMapper<AIMessageRequest, AIMessage> _aIMessageMapper;
        private readonly ILogger<AIManager> _logger;
        private readonly IAIMessageDispatcherFactory _aIMessageDispatcherFactory;

        public event Action<Message> OnSendMessage;
        public Guid AIId { get; }

        public AIManager(Guid aiId, IAIController aiController,
            IChatService chatService, IAIMessageService aiMessageService, 
            IResponseMapper<AIMessageRequest, AIMessage> aIMessageMapper,
            Dictionary<Guid, AIMessageDispatcher> dialogs,IAIMessageDispatcherFactory aIMessageDispatcherFactory,
            ILogger<AIManager> logger)
        {
            _aiController = aiController ?? throw new ArgumentNullException(nameof(aiController));
            _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
            _aiMessageService = aiMessageService ?? throw new ArgumentNullException(nameof(aiMessageService));
            _aIMessageMapper = aIMessageMapper ?? throw new ArgumentNullException(nameof(aIMessageMapper));
            _aIMessageDispatcherFactory = aIMessageDispatcherFactory ?? throw new ArgumentNullException(nameof(aIMessageDispatcherFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));

            foreach (var dispatcher in _dialogs.Values)
            {
                dispatcher.SendMessage += SendMessageToAIAsync;
                dispatcher.OnBufferGroupOverflowing += OnGroupOverflowingAsync;
            }

            AIId = aiId;
            _logger.LogInformation("AIManager initialized with AIId {AIId} and {ChatCount} chat dialogs.", AIId, _dialogs.Count);
        }

        public void CreateDialog(Guid chatId)
        {
            if (!_dialogs.ContainsKey(chatId))
            {
                var dispatcher = _aIMessageDispatcherFactory.Create();
                dispatcher.SendMessage += SendMessageToAIAsync;
                dispatcher.OnBufferGroupOverflowing += OnGroupOverflowingAsync;
                _dialogs.Add(chatId, dispatcher);
                _chatService.AddChatTokenUsage(chatId);

                _logger.LogInformation("Created new AI dialog for chat {ChatId}.", chatId);
            }
        }

        public void EndDialog(Guid chatId)
        {
            if (_dialogs.TryGetValue(chatId, out var dispatcher))
            {
                dispatcher.SendMessage -= SendMessageToAIAsync;
                dispatcher.OnBufferGroupOverflowing -= OnGroupOverflowingAsync;
                _dialogs.Remove(chatId);

                _logger.LogInformation("Ended AI dialog for chat {ChatId}.", chatId);
            }
        }

        public async Task SendMessageAsync(Guid chatId, string message)
        {
            if (!_dialogs.TryGetValue(chatId, out var dispatcher))
            {
                _logger.LogWarning("Dialog for chat {ChatId} not found when sending message.", chatId);
                throw new InvalidOperationException($"Dialog for chat {chatId} not found");
            }

            var messageFromUser = _aiMessageService.AddAIMessage(
                new AIMessage(default, chatId, "user", message), "message");

            await dispatcher.AddMessageAsync(messageFromUser);

            var messages = await dispatcher.GetContextAsync();
            messages.Insert(0, new AIMessage(default, chatId, "system",
                "You are a human-like conversational AI. Respond in a friendly and concise manner. Do not reveal that you are a program."));

            var response = await SendMessageToAIAsync(messages);
            var responseFromAI = _aiMessageService.AddAIMessage(
                new AIMessage(default, chatId, "assistant", response), "message");

            await dispatcher.AddMessageAsync(responseFromAI);

            _logger.LogInformation("Message sent to AI and response received for chat {ChatId}.", chatId);

            OnSendMessage?.Invoke(new Message()
            {
                Chat = responseFromAI.ChatId,
                Text = responseFromAI.Content,
                Sender = AIId
            });
        }

        private async Task<string> SendMessageToAIAsync(List<AIMessage> aiMessages)
        {
            var info = await SendMessageWithRetryAsync(aiMessages);
            _chatService.UseToken(aiMessages[0].ChatId, info.TotalTokensUsed);

            _logger.LogInformation("AI response processed. Tokens used: {TotalTokens}", info.TotalTokensUsed);
            return info.Answer;
        }

        private async Task<AIMessage> OnGroupOverflowingAsync(List<AIMessage> messages)
        {
            try
            {
                var systemMessage = new AIMessage(default, messages[0].ChatId, "system",
                    "Compress the following messages into a concise summary, keeping key information. " +
                    "Respond in the same language as the messages. Do not add comments, only the summary.");

                messages.Insert(0, systemMessage);
                string response = await SendMessageToAIAsync(messages);

                _aiMessageService.DeleteAIMessages(messages);

                _logger.LogInformation("Compressed message generated for chat {ChatId}.", messages[0].ChatId);

                return _aiMessageService.AddAIMessage(
                    new AIMessage(default, messages[0].ChatId, "system", response), "compressedMessage");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnGroupOverflowing for chat {ChatId}.", messages[0].ChatId);
                throw;
            }
        }

        private async Task<AIMessageResponse> SendMessageWithRetryAsync(List<AIMessage> messages)
        {
            AIMessageResponse? response;
            do
            {
                response = await _aiController.SendMessageAsync(GetAIMessageRequests(messages));
            } while (response == null);

            return response;
        }

        private List<AIMessageRequest> GetAIMessageRequests(IReadOnlyCollection<AIMessage> aIMessages)
        {
            return aIMessages.Select(m => _aIMessageMapper.ToDTO(m)).ToList();
        }
    }
}
