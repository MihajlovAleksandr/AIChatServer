using AIChatServer.Entities.Connection.Interfaces;
using AIChatServer.Entities.User.ServerUsers.Implementations;

namespace AIChatServer.Entities.User.ServerUsers.Interfaces
{
    public interface IKnownUserFactory
    {
        KnownUser Create(User user, IConnection connection);
        KnownUser Create(User user, IReadOnlyCollection<IConnection> connections);
    }
}
