using AIChatServer.Entities.Connection;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Utils;
using AIChatServer.Utils.Interfaces;

namespace AIChatServer.Managers.Implementations.CommandHandlers.MainManagerCommands
{
    public class SyncDBCommandHandler(ISyncManager syncService, ISerializer serializer) : ICommandHandler
    {
        private readonly ISyncManager _syncService = syncService 
            ?? throw new ArgumentNullException(nameof(syncService));
        private readonly ISerializer _serializer = serializer
            ?? throw new ArgumentNullException(nameof(serializer));

        public string Operation => "SyncDB";

        public async Task HandleAsync(object sender, Command command)
        {
            var knownUser = (IServerUser)sender;
            Guid syncDBUserId = knownUser.User.Id;
            await CommandSender.SendCommandAsync(command.Sender, _syncService.GetSyncCommand(syncDBUserId, DateTime.MinValue), _serializer);
        }
    }
}
