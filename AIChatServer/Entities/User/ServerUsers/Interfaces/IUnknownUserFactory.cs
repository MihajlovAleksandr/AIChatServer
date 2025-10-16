using AIChatServer.Entities.Connection.Interfaces;
using AIChatServer.Entities.User.ServerUsers.Implementations;

namespace AIChatServer.Entities.User.ServerUsers.Interfaces
{
    public interface IUnknownUserFactory
    {
        UnknownUser Create(Guid id, IConnection connection);
    }
}
