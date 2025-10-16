using AIChatServer.Entities.Connection;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Managers.Interfaces;

namespace AIChatServer.Managers.Implementations.CommandHandlers.MainManagerCommands
{
    public class StopSearchingChatCommandHandler(IChatManager chatManager) : ICommandHandler
    {
        private readonly IChatManager _chatManager = chatManager
            ?? throw new ArgumentNullException(nameof(chatManager));

        public string Operation => "StopSearchingChat";

        public async Task HandleAsync(object sender, Command command)
        {
            var knownUser = (IServerUser)sender;
            _chatManager.StopSearchingChat(knownUser.User.Id);

            CommandResponse stopSearchChatCommand = new CommandResponse("SearchChat", new SearchChatResponse(false));
            await knownUser.SendCommandAsync(stopSearchChatCommand);
        }
    }
}
