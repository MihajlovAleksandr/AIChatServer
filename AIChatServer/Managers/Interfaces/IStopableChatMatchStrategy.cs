namespace AIChatServer.Managers.Interfaces
{
    public interface IStopableChatMatchStrategy : IChatMatchStrategy
    {
        public void StopSearching(Guid userId);
        public bool IsSearching(Guid userId);
    }
}
