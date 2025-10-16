using AIChatServer.Entities.Connection;
using AIChatServer.Entities.DTO.Request;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Managers.Interfaces;

namespace AIChatServer.Managers.Implementations.CommandHandlers.MainManagerCommands
{
    public class SearchChatCommandHandler(IChatManager chatManager) : ICommandHandler
    {
        private readonly IChatManager _chatManager = chatManager ??
            throw new ArgumentNullException(nameof(chatManager));

        public string Operation => "SearchChat";

        public async Task HandleAsync(object sender, Command command)
        {
            var knownUser = (IServerUser)sender;
            SearchChatRequest searchChatRequest = command.GetData<SearchChatRequest>() ?? throw new ArgumentNullException("SearchChatRequest");
            CommandResponse searchChatCommand = new CommandResponse("SearchChat", new SearchChatResponse(true));
            await knownUser.SendCommandAsync(searchChatCommand);
            await _chatManager.SearchChatAsync(knownUser.User, searchChatRequest.ChatType);
        }
    }
}
