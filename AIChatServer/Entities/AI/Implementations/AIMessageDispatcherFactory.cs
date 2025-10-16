using AIChatServer.Entities.AI.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Entities.AI.Implementations
{
    public class AIMessageDispatcherFactory(
        (int, int) messageBufferSizes,
        (int, int) compressedMessageBufferSizes,
        ILogger<AIMessageDispatcher> logger,
        IAIMessageGroupFactory factory) : IAIMessageDispatcherFactory
    {
        private readonly (int, int) _messageBufferSizes = messageBufferSizes;
        private readonly (int, int) _compressedMessageBufferSizes = compressedMessageBufferSizes;
        private readonly ILogger<AIMessageDispatcher> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IAIMessageGroupFactory _factory = factory ?? throw new ArgumentNullException(nameof(factory));

        public AIMessageDispatcher Create()
        {
            return new AIMessageDispatcher(
                _factory.Create(_messageBufferSizes),
                _factory.Create(_compressedMessageBufferSizes),
                _logger);
        }
    }
}
