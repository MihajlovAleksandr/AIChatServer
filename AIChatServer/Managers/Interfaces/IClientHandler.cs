using System.Net;

namespace AIChatServer.Managers.Interfaces
{
    public interface IClientHandler
    {
        Task HandleClientAsync(HttpListenerContext context);
    }
}