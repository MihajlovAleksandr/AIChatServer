using AIChatServer.Entities.Connection;

namespace AIChatServer.Repositories.Interfaces
{
    public interface IConnectionRepository
    {
        Guid AddConnection(string device);
        ConnectionInfo GetConnectionInfo(Guid connectionId, Guid defaultUserId);
        List<ConnectionInfo> GetAllUserConnections(Guid userId);
        bool VerifyConnection(Guid id, Guid userId, string device, out DateTime lastConnection);
        ConnectionInfo RemoveConnection(Guid id);
        bool SetLastConnection(Guid connectionId, bool isOnline);
        void UpdateConnection(Guid connectionId, Guid userId);
        int[] GetConnectionCount(Guid userId);
        void DeleteUnknownConnection(Guid id);
        DateTime? GetLastUserOnline(Guid userId);
    }
}