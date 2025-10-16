using AIChatServer.Entities.AI.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Entities.AI.Implementations
{
    public class AIMessageDispatcher : IMessageDispatcher, IDisposable
    {
        private readonly IAIMessageGroup _mainMessageGroup;
        private readonly IAIMessageGroup _compressedMessageGroup;
        private readonly SemaphoreSlim _processingSemaphore = new(1, 1);
        private readonly ILogger<AIMessageDispatcher> _logger;
        private bool _disposed;

        public event Func<List<AIMessage>, Task<string>>? SendMessage;
        public event Func<List<AIMessage>, Task<AIMessage>>? OnBufferGroupOverflowing;

        public AIMessageDispatcher(
            IAIMessageGroup mainMessageGroup,
            IAIMessageGroup compressedMessageGroup,
            ILogger<AIMessageDispatcher> logger)
        {
            _mainMessageGroup = mainMessageGroup 
                ?? throw new ArgumentNullException(nameof(mainMessageGroup));
            _compressedMessageGroup = compressedMessageGroup 
                ?? throw new ArgumentNullException(nameof(compressedMessageGroup));
            _logger = logger 
                ?? throw new ArgumentNullException(nameof(logger));

            _mainMessageGroup.OnBufferOverflowing += OnGroupOverflowing;
            _compressedMessageGroup.OnBufferOverflowing += OnGroupOverflowing;

            _logger.LogInformation("AIMessageDispatcher initialized");
        }

        public async Task SetMainMessagesAsync(IReadOnlyCollection<AIMessage> messages)
        {
            await SetMessagesAsync(_mainMessageGroup, messages, "main");
        }

        public async Task SetCompressedMessagesAsync(IReadOnlyCollection<AIMessage> messages)
        {
            await SetMessagesAsync(_compressedMessageGroup, messages, "compressed");
        }

        private async Task SetMessagesAsync(IAIMessageGroup messageGroup, IReadOnlyCollection<AIMessage> messages, string groupName)
        {
            await _processingSemaphore.WaitAsync();
            try
            {
                await messageGroup.SetMessagesAsync(messages);
                _logger.LogDebug("Set {Count} messages to {GroupName} group.", messages.Count, groupName);
            }
            finally
            {
                _processingSemaphore.Release();
            }
        }

        private async Task OnGroupOverflowing(List<AIMessage> messages)
        {
            if (OnBufferGroupOverflowing == null)
            {
                _logger.LogWarning("Overflow detected in message group, but no handler is subscribed.");
                return;
            }

            try
            {
                var compressedMessage = await OnBufferGroupOverflowing.Invoke(messages);
                await _compressedMessageGroup.AddMessageAsync(compressedMessage);
                _logger.LogInformation("Overflow handled: compressed message added to compressed group.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while handling buffer overflow in AIMessageDispatcher.");
            }
        }

        public async Task<List<AIMessage>> GetContextAsync()
        {
            await _processingSemaphore.WaitAsync();
            try
            {
                var mainMessages = _mainMessageGroup.GetMessages();
                var compressedMessages = _compressedMessageGroup.GetMessages();

                var allMessages = new List<AIMessage>(compressedMessages.Count + mainMessages.Count);
                allMessages.AddRange(compressedMessages);
                allMessages.AddRange(mainMessages);

                _logger.LogDebug("Context retrieved: {CompressedCount} compressed, {MainCount} main messages.",
                    compressedMessages.Count, mainMessages.Count);

                return allMessages;
            }
            finally
            {
                _processingSemaphore.Release();
            }
        }

        public async Task AddMessageAsync(AIMessage aiMessage)
        {
            await _processingSemaphore.WaitAsync();
            try
            {
                await _mainMessageGroup.AddMessageAsync(aiMessage);
                _logger.LogDebug("Message added to main group. Current main size: {Count}.", _mainMessageGroup.GetMessages().Count);
            }
            finally
            {
                _processingSemaphore.Release();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            _mainMessageGroup.OnBufferOverflowing -= OnGroupOverflowing;
            _compressedMessageGroup.OnBufferOverflowing -= OnGroupOverflowing;

            _processingSemaphore.Dispose();
            _disposed = true;

            _logger.LogInformation("AIMessageDispatcher disposed.");
        }
    }
}
