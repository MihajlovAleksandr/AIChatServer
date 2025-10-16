using AIChatServer.Entities.Connection.Interfaces;
using AIChatServer.Entities.User.ServerUsers.Interfaces;

namespace AIChatServer.Managers.Interfaces
{
    public interface IConnectionManager
    {
        bool ConnectUser(Guid userId, IServerUser serverUser);
        IServerUser? GetUser(Guid userId);
        bool DisconnectUser(Guid id, out IServerUser? serverUser);
        bool ReconnectUser(Guid oldId, Guid newUserId);
        Task<IServerUser?> СreateUserAsync(Guid userId, bool needToRefreshToken, IConnection connection);
        IServerUser[] GetConnectedUsers(Guid[] users);
    }
}
