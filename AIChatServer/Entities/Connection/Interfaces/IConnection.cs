namespace AIChatServer.Entities.Connection.Interfaces
{
    public interface IConnection : IAsyncDisposable
    {
        Guid Id { get; set; }
        event EventHandler<Command> CommandGot;
        event EventHandler Disconnected;
        Task SendCommandAsync(byte[] command);
    }
}
