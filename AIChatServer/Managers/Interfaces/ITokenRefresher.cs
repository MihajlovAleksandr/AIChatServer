using AIChatServer.Entities.Connection.Interfaces;

namespace AIChatServer.Managers.Interfaces
{
    public interface ITokenRefresher
    {
        Task SendRefreshCommandAsync(IConnection connection,  Guid userId);
    }
}
