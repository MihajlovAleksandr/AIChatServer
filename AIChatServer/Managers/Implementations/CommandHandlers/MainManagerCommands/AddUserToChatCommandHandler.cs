using AIChatServer.Entities.Chats;
using AIChatServer.Entities.Connection;
using AIChatServer.Entities.DTO.Request;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Managers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatServer.Managers.Implementations.CommandHandlers.MainManagerCommands
{
    class AddUserToChatCommandHandler(IAddUserStrategiesHandler chatUserAdder,
        IChatManager chatManager) : ICommandHandler
    {

        private readonly IAddUserStrategiesHandler _chatUserAdder = chatUserAdder
                    ?? throw new ArgumentNullException(nameof(chatUserAdder));
        private readonly IChatManager _chatManager = chatManager
            ?? throw new ArgumentNullException(nameof(chatManager));

        public string Operation => "AddUserToChat";

        public async Task HandleAsync(object sender, Command command)
        {
            IServerUser serverUser = sender as IServerUser
                ?? throw new ArgumentNullException(nameof(sender));
            AddUserToChatRequest addUserRequest = command.GetData<AddUserToChatRequest>()
                ?? throw new ArgumentNullException(nameof(addUserRequest));

            if (!await _chatUserAdder.AddUser(serverUser.User, addUserRequest.ChatType, _chatManager,
                addUserRequest.ChatMatchPredicate))
            {
                CommandResponse addUserOrChatCommand = new CommandResponse("UserAdding", new UserAddingResponse(true, null));
                await serverUser.SendCommandAsync(addUserOrChatCommand);
            }
        }
    }
}
