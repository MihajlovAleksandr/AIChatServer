using System.Net.WebSockets;

namespace AIChatServer.Entities.Connection.Interfaces
{
    public interface IConnectionFactory
    {
        IConnection Create(Guid id, WebSocket webSocket);
    }
}
