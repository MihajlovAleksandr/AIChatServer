namespace AIChatServer.Entities.AI.Interfaces
{
    public interface IAIMessageGroup
    {
        event Func<List<AIMessage>, Task> OnBufferOverflowing;
        Task AddMessageAsync(AIMessage message);
        Task SetMessagesAsync(IReadOnlyCollection<AIMessage> messages);
        List<AIMessage> GetMessages();
    }
}