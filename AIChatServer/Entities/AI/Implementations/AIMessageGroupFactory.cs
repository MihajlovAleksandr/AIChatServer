using AIChatServer.Entities.AI.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Entities.AI.Implementations
{
    public class AIMessageGroupFactory(ILogger<AIMessageGroup> logger) : IAIMessageGroupFactory
    {
        private readonly ILogger<AIMessageGroup> _logger = logger;

        public AIMessageGroup Create((int, int) messageGroupSize)
        {
            return new AIMessageGroup(messageGroupSize.Item1, messageGroupSize.Item2, _logger);
        }
    }
}
