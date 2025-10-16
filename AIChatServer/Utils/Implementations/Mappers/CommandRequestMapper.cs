using AIChatServer.Entities.Connection;
using AIChatServer.Utils.Interfaces.Mapper;

namespace AIChatServer.Utils.Implementations.Mappers
{
    public class CommandRequestMapper : IRequestMapper<CommandRequestWithSender, Command>
    {
        public Command ToModel(CommandRequestWithSender request)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Command);
            ArgumentNullException.ThrowIfNull(request.Command.Operation);
            ArgumentNullException.ThrowIfNull(request.Sender);

            return new Command(request.Command.Operation, request.Sender, request.Command.Data);
        }
    }
}
