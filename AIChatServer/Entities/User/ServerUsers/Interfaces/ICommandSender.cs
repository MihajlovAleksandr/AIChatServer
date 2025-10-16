using AIChatServer.Entities.DTO.Response;

namespace AIChatServer.Entities.User.ServerUsers.Interfaces
{
    public interface ICommandSender
    {
        Task SendCommandAsync(CommandResponse command);
        Task SendCommandAsync(Guid connectionId, CommandResponse command);
    }
}