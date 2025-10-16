using AIChatServer.Entities.Connection;

namespace AIChatServer.Entities.User.ServerUsers.Interfaces
{
    public interface IConnectionEventHandler
    {
        event EventHandler<Command>? GotCommand;
        event EventHandler? Disconnected;
    }
}
