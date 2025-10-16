namespace AIChatServer.Entities.AI.Interfaces
{
    public interface IMessageDispatcher
    {
        event Func<List<AIMessage>, Task<string>> SendMessage;
        event Func<List<AIMessage>, Task<AIMessage>> OnBufferGroupOverflowing;
        Task SetMainMessagesAsync(IReadOnlyCollection<AIMessage> messages);
        Task SetCompressedMessagesAsync(IReadOnlyCollection<AIMessage> messages);
        Task<List<AIMessage>> GetContextAsync();
        Task AddMessageAsync(AIMessage aIMessage);
    }
}