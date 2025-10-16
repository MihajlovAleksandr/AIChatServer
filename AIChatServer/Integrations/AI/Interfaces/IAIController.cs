using AIChatServer.Integrations.AI.DTO;

namespace AIChatServer.Integrations.AI.Interfaces
{
    public interface IAIController
    {
        Task<AIMessageResponse?> SendMessageAsync(List<AIMessageRequest> aiResponse);
    }
}
