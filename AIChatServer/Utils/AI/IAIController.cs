using AIChatServer.Entities.AI;
using AIChatServer.Entities.Chats;

namespace AIChatServer.Utils.AI
{
    public interface IAIController
    {
        Task<AIResponseInfo?> SendMessageAsync(List<AIMessage> aiResponse);
    }
}
