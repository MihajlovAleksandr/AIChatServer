using AIChatServer.Entities.Chats;
using AIChatServer.Entities.Connection;
using AIChatServer.Entities.DTO.Request;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Managers.Interfaces;

namespace AIChatServer.Managers.Implementations.CommandHandlers.MainManagerCommands
{
    public class AddOtherUserToChatCommandHandler(IAddUserStrategiesHandler chatUserAdder, IChatManager chatManager) : ICommandHandler
    {
        private readonly IAddUserStrategiesHandler _chatUserAdder = chatUserAdder
            ?? throw new ArgumentNullException(nameof(chatUserAdder));
        private readonly IChatManager _chatManager = chatManager
            ?? throw new ArgumentNullException(nameof(chatManager));

        public string Operation => "AddOtherUserToChat";

        public async Task HandleAsync(object sender, Command command)
        {
            IServerUser serverUser = sender as IServerUser
                ?? throw new ArgumentNullException(nameof(sender));
            AddOtherUserToChatRequest addUserOrChatRequest = command.GetData<AddOtherUserToChatRequest>()
                ?? throw new ArgumentNullException(nameof(addUserOrChatRequest));

            Chat chat = _chatManager.GetChat(addUserOrChatRequest.ChatId) 
                ?? throw new ArgumentNullException(nameof(chat));

            if (!await _chatUserAdder.AddChat(serverUser.User, _chatManager,
                chat, addUserOrChatRequest.ChatMatchPredicate))
            {
                CommandResponse addUserOrChatCommand = new CommandResponse("UserAdding", new UserAddingResponse(true, chat.Id));
                await serverUser.SendCommandAsync(addUserOrChatCommand);
            }
        }
    }
}
