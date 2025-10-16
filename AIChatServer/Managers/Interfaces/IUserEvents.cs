using AIChatServer.Entities.Connection;

namespace AIChatServer.Managers.Interfaces
{
    public interface IUserEvents
    {
        event EventHandler<Command> CommandGot;
        event EventHandler<bool> ConnectionChanged;
    }

}
