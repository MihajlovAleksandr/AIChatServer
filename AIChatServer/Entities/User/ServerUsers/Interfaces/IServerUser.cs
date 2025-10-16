using AIChatServer.Entities.Connection.Interfaces;

namespace AIChatServer.Entities.User.ServerUsers.Interfaces
{
    public interface IServerUser : IUserConnection, ICommandSender, IConnectionEventHandler, IDisposable
    {
        User User { get; }
        void AddConnections(IEnumerable<IConnection> connections);
    }
}
