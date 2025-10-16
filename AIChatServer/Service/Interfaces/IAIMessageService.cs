using AIChatServer.Entities.AI;
using AIChatServer.Entities.AI.Implementations;

namespace AIChatServer.Service.Interfaces
{
    public interface IAIMessageService
    {
        AIMessage AddAIMessage(AIMessage aIMessage, string type);
        Task<Dictionary<Guid, AIMessageDispatcher>> GetAIMessagesByChatAsync(List<Guid> chatIds);
        Task<AIMessageDispatcher> GetAIMessagesByChatAsync(Guid chatId);
        bool DeleteAIMessages(List<AIMessage> messages);
    }
}