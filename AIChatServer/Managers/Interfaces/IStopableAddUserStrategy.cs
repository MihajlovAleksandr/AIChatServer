namespace AIChatServer.Managers.Interfaces
{
    public interface IStopableAddUserStrategy : IAddUserStrategy
    {
        void StopSearching(Guid userId);
        Guid? IsSearching(Guid userId);
    }
}
