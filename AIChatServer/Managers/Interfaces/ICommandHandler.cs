using AIChatServer.Entities.Connection;

namespace AIChatServer.Managers.Interfaces
{
    public interface ICommandHandler
    {
        string Operation { get; }
        Task HandleAsync(object sender, Command command);
    }
}

