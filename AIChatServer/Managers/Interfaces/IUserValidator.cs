using AIChatServer.Entities.Connection.Interfaces;

namespace AIChatServer.Managers.Interfaces
{
    public interface IUserValidator
    {
        Task<bool> ValidateAsync(Guid userId, bool needToRefreshToken, IConnection connection);
    }
}

