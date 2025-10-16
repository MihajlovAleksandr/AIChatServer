using AIChatServer.Entities.Connection.Interfaces;
using AIChatServer.Entities.DTO.Request;

namespace AIChatServer.Entities.Connection
{
    public class CommandRequestWithSender(CommandRequest command, IConnection sender)
    {
        public CommandRequest Command { get; } = command;
        public IConnection Sender { get; } = sender;
    }
}
