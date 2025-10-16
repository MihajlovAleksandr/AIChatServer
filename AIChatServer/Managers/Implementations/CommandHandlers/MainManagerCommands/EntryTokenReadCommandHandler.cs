using AIChatServer.Entities.Connection;
using AIChatServer.Entities.DTO.Request;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Utils.Interfaces;

namespace AIChatServer.Managers.Implementations.CommandHandlers.MainManagerCommands
{
    public class EntryTokenReadCommandHandler(IEntryTokenManager entryTokenManager, IUserManager userManager) : ICommandHandler
    {
        private readonly IEntryTokenManager _entryTokenManager = entryTokenManager
            ?? throw new ArgumentNullException(nameof(entryTokenManager));
        private readonly IUserManager _userManager = userManager
            ?? throw new ArgumentNullException(nameof(userManager));

        public string Operation => "EntryTokenRead";

        public async Task HandleAsync(object sender, Command command)
        {
            var knownUser = (IServerUser)sender;
            EntryTokenRequest entryTokenRequest = command.GetData<EntryTokenRequest>() ?? throw new ArgumentNullException("EntryTokenRequest");

            if (_entryTokenManager.ValidateEntryToken(entryTokenRequest.Token, out Guid userId))
            {
                await _userManager.KnowUserAsync(userId, knownUser);
            }
        }
    }
}
