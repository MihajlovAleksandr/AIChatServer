using AIChatServer.Entities.Connection.Interfaces;

namespace AIChatServer.Entities.User.ServerUsers.Interfaces
{
    public interface IUserConnection
    {
        void AddConnection(IConnection connection);
        IConnection? RemoveConnection(Guid id);
        IReadOnlyCollection<IConnection> GetConnections();
        IConnection? GetConnection(Guid id);
    }
}
