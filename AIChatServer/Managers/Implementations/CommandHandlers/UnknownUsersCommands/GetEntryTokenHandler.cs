using AIChatServer.Entities.Connection;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Utils;
using AIChatServer.Utils.Interfaces;

namespace AIChatServer.Managers.Implementations.CommandHandlers.UnknownUsersCommands
{
    public class GetEntryTokenHandler(ISerializer serializer, IEntryTokenManager entryTokenManager) : ICommandHandler
    {
        private readonly ISerializer _serializer = serializer
            ?? throw new ArgumentNullException(nameof(serializer));
        private readonly IEntryTokenManager _entryTokenManager = entryTokenManager
            ?? throw new ArgumentNullException(nameof(entryTokenManager));

        public string Operation => "GetEntryToken";

        public async Task HandleAsync(object sender, Command command)
        {
            IUnknownUser userContext = sender as IUnknownUser ?? throw new Exception("Only UnknownUser can be sender");
            CommandResponse entryTokenCommand = new CommandResponse("EntryToken",
                new EntryTokenResponse(_entryTokenManager.GenerateEntryToken(userContext.User.Id)));
            await CommandSender.SendCommandAsync(command.Sender, entryTokenCommand, _serializer);
        }
    }
}
