using AIChatServer.Entities.Connection.Interfaces;
using AIChatServer.Entities.Connection;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Entities.DTO.Response;

namespace AIChatServer.Managers.Interfaces
{
    public interface IUserManager
    {
        event EventHandler<Command> CommandGot;
        event EventHandler<bool> OnConnectionEvent;

        Task<IUnknownUser> CreateUnknownUser(IConnection connection);
        Task KnowUserAsync(Guid unknownUserId, IServerUser knownUser);
        Task SendCommandAsync(IDictionary<Guid, CommandResponse> userCommandPairs);
    }
}