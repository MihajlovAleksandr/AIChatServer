using AIChatServer.Entities.Connection;
using AIChatServer.Entities.DTO.Request;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Managers.Interfaces;

namespace AIChatServer.Managers.Implementations.CommandHandlers.MainManagerCommands
{
    public class SearchChatCommandHandler(IChatMatchStrategiesHandler chatMatcher, IChatLifecycleManager chatCreator) : ICommandHandler
    {
        private readonly IChatMatchStrategiesHandler _chatMatcher = chatMatcher
            ?? throw new ArgumentNullException(nameof(chatMatcher));
        private readonly IChatLifecycleManager _chatCreator = chatCreator
            ?? throw new ArgumentNullException(nameof(chatCreator));

        public string Operation => "SearchChat";

        public async Task HandleAsync(object sender, Command command)
        {
            var knownUser = (IServerUser)sender;
            SearchChatRequest searchChatRequest = command.GetData<SearchChatRequest>() ?? throw new ArgumentNullException("SearchChatRequest");
            CommandResponse searchChatCommand = new CommandResponse("SearchChat", new SearchChatResponse(true));
            await knownUser.SendCommandAsync(searchChatCommand);
            await _chatMatcher.SearchChatAsync(knownUser.User, searchChatRequest.ChatType, 
                _chatCreator, searchChatRequest.ChatMatchPredicate );
        }
    }
}
