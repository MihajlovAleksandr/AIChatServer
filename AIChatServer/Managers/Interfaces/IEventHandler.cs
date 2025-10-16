namespace AIChatServer.Managers.Interfaces
{
    public interface IEventHandler
    {
        Task HandleAsync(object? sender, object? args);
        void Subscribe() { }
    }
}
