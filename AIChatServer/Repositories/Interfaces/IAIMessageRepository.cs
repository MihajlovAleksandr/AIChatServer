using AIChatServer.Entities.AI;

namespace AIChatServer.Repositories.Interfaces
{
    public interface IAIMessageRepository
    {
        AIMessage AddAIMessage(Guid chatId, string content, string role, string type);
        IDictionary<Guid, (IReadOnlyCollection<AIMessage>, IReadOnlyCollection<AIMessage>)> GetAIMessagesByChat(IReadOnlyCollection<Guid> chatIds);
        bool DeleteAIMessages(Guid[] messages);

    }
}