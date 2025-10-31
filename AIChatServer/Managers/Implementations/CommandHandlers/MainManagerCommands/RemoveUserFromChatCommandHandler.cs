using AIChatServer.Entities.Connection;
using AIChatServer.Entities.DTO.Request;
using AIChatServer.Managers.Interfaces;

namespace AIChatServer.Managers.Implementations.CommandHandlers.MainManagerCommands
{
    public class RemoveUserFromChatCommandHandler(IChatUserManager chatUserManager) : ICommandHandler
    {
        private readonly IChatUserManager _chatUserManager = chatUserManager
                ?? throw new ArgumentNullException(nameof(chatUserManager));

        public string Operation => "RemoveUserFromChat";

        public Task HandleAsync(object sender, Command command)
        {
            RemoveUserFromChatRequest request = command.GetData<RemoveUserFromChatRequest>()
                ?? throw new ArgumentNullException(nameof(request));
            _chatUserManager.RemoveUserFromChat(request.UserId, request.ChatId);
            return Task.CompletedTask;
        }
    }
}
