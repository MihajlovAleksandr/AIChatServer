using AIChatServer.Entities.Connection.Interfaces;
using AIChatServer.Entities.User.ServerUsers.Interfaces;

namespace AIChatServer.Managers.Interfaces
{
    public interface IConnectionStorage
    {
        bool Add(Guid userId, IServerUser serverUser);
        bool Add(Guid userId, IConnection connection, out IServerUser serverUser);
        bool Remove(Guid userId, out IServerUser? serverUser);
        bool TryGet(Guid userId, out IServerUser? serverUser);
    }
}
