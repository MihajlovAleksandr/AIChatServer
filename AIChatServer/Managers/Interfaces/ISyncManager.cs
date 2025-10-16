using AIChatServer.Entities.DTO.Response;

namespace AIChatServer.Managers.Interfaces
{
    public interface ISyncManager
    {
        CommandResponse GetSyncCommand(Guid userId, DateTime lastOnline);
    }
}
