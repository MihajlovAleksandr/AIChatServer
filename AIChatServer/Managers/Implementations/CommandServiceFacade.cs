using AIChatServer.Entities.Chats;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Entities.User;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Service.Interfaces;
using AIChatServer.Utils.Interfaces.Mapper;

namespace AIChatServer.Managers.Implementations
{
    public class CommandServiceFacade(IUserManager userManager, IConnectionService connectionService)
    {

        private readonly IUserManager _userManager = userManager
            ?? throw new ArgumentNullException(nameof(userManager));
        private readonly IConnectionService _connectionService = connectionService
            ?? throw new ArgumentNullException(nameof(connectionService));

        public async Task SendChatCommandAsync(
            string commandName,
            Chat chat,
            IResponseMapper<ChatResponse, ChatWithUserContext> mapper)
        {
            var userCommands = new Dictionary<Guid, CommandResponse>();

            foreach (var (userId, _) in chat.UsersWithData)
            {
                var context = new ChatWithUserContext(chat, userId);
                var dto = mapper.ToDTO(context) ?? throw new InvalidOperationException("ChatResponse mapping failed");
                userCommands[userId] = new CommandResponse(commandName, dto);
            }

            await _userManager.SendCommandAsync(userCommands);
        }
        public async Task SendUserDataCommandAsync(UserData userData, Guid addedUserId, Chat chat,
            IResponseMapper<UserDataResponse, UserData> userDataMapper,
            IResponseMapper<ChatResponse, ChatWithUserContext> chatMapper)
        {
            UserDataResponse userDataResponse = userDataMapper.ToDTO(userData);

            Dictionary<Guid, CommandResponse> commands = new()
            {
                { addedUserId, new CommandResponse("CreateChat", chatMapper.ToDTO(new ChatWithUserContext(chat, addedUserId))) }
            };

            CommandResponse addUserCommand = new CommandResponse("AddUserToChat",
                new AddUserToChatResponse(chat.Id, addedUserId, userDataResponse,
                    _connectionService.GetLastUserOnline(addedUserId) == null));

            foreach (Guid userId in chat.UsersWithData.Keys)
            {
                if (userId != addedUserId)
                    commands.Add(userId, addUserCommand);
            }
            await _userManager.SendCommandAsync(commands);
        }

        public async Task SendUserRemoveCommandAsync(Guid removedUserId, Chat chat)
        {
            Dictionary<Guid, CommandResponse> commands = new();
            foreach (Guid userId in chat.UsersWithData.Keys)
            {
                commands.Add(userId, new CommandResponse("RemoveUserFromChat", new RemoveUserFromChatResponse(removedUserId, chat.Id)));
            }
            commands.Add(removedUserId, new CommandResponse("DeleteChat", new DeleteChatResponse(chat.Id)));
            await _userManager.SendCommandAsync(commands);
        }
    }
}
