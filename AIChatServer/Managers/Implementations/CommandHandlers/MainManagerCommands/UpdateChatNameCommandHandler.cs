using AIChatServer.Entities.Connection;
using AIChatServer.Entities.DTO.Request;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Service.Interfaces;

namespace AIChatServer.Managers.Implementations.CommandHandlers.MainManagerCommands
{
    public class UpdateChatNameCommandHandler(IChatService chatService) : ICommandHandler
    {
        private readonly IChatService _chatService = chatService
            ?? throw new ArgumentNullException(nameof(chatService));

        public string Operation => "UpdateChatName";

        public async Task HandleAsync(object sender, Command command)
        {
            var knownUser = (IServerUser)sender;
            UpdateChatNameRequest updateChatNameRequest = command.GetData<UpdateChatNameRequest>() 
                ?? throw new ArgumentNullException("UpdateChatNameRequest");

            _chatService.UpdateChatName(knownUser.User.Id, updateChatNameRequest.ChatId, updateChatNameRequest.Name);

            CommandResponse updateChatNameCommand = new CommandResponse("UpdateChatName",
                new UpdateChatNameResponse(updateChatNameRequest.ChatId, updateChatNameRequest.Name));
            await knownUser.SendCommandAsync(updateChatNameCommand);
        }
    }
}
