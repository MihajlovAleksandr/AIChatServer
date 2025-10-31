using AIChatServer.Entities.Connection;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Managers.Interfaces;

namespace AIChatServer.Managers.Implementations.CommandHandlers.MainManagerCommands
{
    public class StopSearchingChatCommandHandler(IChatMatchStrategiesHandler chatMatcher) : ICommandHandler
    {
        private readonly IChatMatchStrategiesHandler _chatMatcher = chatMatcher
            ?? throw new ArgumentNullException(nameof(chatMatcher));

        public string Operation => "StopSearchingChat";

        public async Task HandleAsync(object sender, Command command)
        {
            var knownUser = (IServerUser)sender;
            _chatMatcher.StopSearchingChat(knownUser.User.Id);

            CommandResponse stopSearchChatCommand = new CommandResponse("SearchChat", new SearchChatResponse(false));
            await knownUser.SendCommandAsync(stopSearchChatCommand);
        }
    }
}
