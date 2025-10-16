using AIChatServer.Entities.Connection;
using AIChatServer.Entities.DTO.Request;
using AIChatServer.Managers.Interfaces;

namespace AIChatServer.Managers.Implementations.CommandHandlers.MainManagerCommands
{
    public class EndChatCommandHandler(IChatManager chatManager) : ICommandHandler
    {
        private readonly IChatManager _chatManager = chatManager ?? throw new ArgumentNullException(nameof(chatManager));

        public string Operation => "EndChat";

        public Task HandleAsync(object sender, Command command)
        {
            EndChatRequest endChatRequest = command.GetData<EndChatRequest>() ?? throw new ArgumentNullException("EndChatRequest");
            _chatManager.EndChat(endChatRequest.ChatId);
            return Task.CompletedTask;
        }
    }
}
