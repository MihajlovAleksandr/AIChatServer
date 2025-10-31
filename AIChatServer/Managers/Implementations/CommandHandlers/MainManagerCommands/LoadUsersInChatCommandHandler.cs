using AIChatServer.Entities.Connection;
using AIChatServer.Entities.User;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Service.Interfaces;
using AIChatServer.Utils;
using AIChatServer.Utils.Interfaces;
using AIChatServer.Entities.DTO.Request;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Entities.Chats;
using AIChatServer.Utils.Interfaces.Mapper;
using AIChatServer.Entities.User.ServerUsers.Interfaces;

namespace AIChatServer.Managers.Implementations.CommandHandlers.MainManagerCommands
{
    public class LoadUsersInChatCommandHandler(IChatService chatService, IChatManager chatManager, IAIManager aiManager,
        ISerializer serializer, IResponseMapper<UserDataResponse, UserData> userDataMapper) : ICommandHandler
    {
        private readonly IChatService _chatService = chatService
           ?? throw new ArgumentNullException(nameof(chatService));
        private readonly IChatManager _chatManager = chatManager
           ?? throw new ArgumentNullException(nameof(chatManager));
        private readonly IAIManager _aiManager = aiManager
           ?? throw new ArgumentNullException(nameof(aiManager));
        private readonly ISerializer _serializer = serializer
           ?? throw new ArgumentNullException(nameof(serializer));
        private readonly IResponseMapper<UserDataResponse, UserData> _userDataMapper = userDataMapper
           ?? throw new ArgumentNullException(nameof(userDataMapper));

        public string Operation => "LoadUsersInChat";

        public async Task HandleAsync(object sender, Command command)
        {
            UsersInChatRequest usersInChatRequest = command.GetData<UsersInChatRequest>() 
                ?? throw new ArgumentNullException("UsersInChatRequest");
            IServerUser serverUser = sender as IServerUser
                ?? throw new ArgumentNullException(nameof(serverUser));
            await CommandSender.SendCommandAsync(command.Sender,
                GetLoadUsersInChatCommand(usersInChatRequest.ChatId, serverUser.User.Id), _serializer);
        }

        private CommandResponse GetLoadUsersInChatCommand(Guid chatId, Guid userId)
        {
            var data = _chatService.LoadUsers(chatId);
            int aiIndex = data.Item1.IndexOf(_aiManager.AIId);
            Chat chat = _chatManager.GetChat(chatId) ?? throw new ArgumentNullException(nameof(chat));

            if (aiIndex != -1)
            {
                data.Item3[aiIndex] = true;

            }
            int currentUserIndex = data.Item1.IndexOf(userId);
            if (chat.Type == ChatType.Random)
            {
                for (int i = 0; i < data.Item2.Count && i< data.Item3.Count; i++)
                {
                    if (i == currentUserIndex) continue;

                    data.Item2[i] = new UserData() { Age = 0, Name = "Random", Gender = Gender.None };
                    data.Item3[i] = true;
                }

            }
            return new CommandResponse("LoadUsersInChat", new UsersInChatResponse(data.Item1, GetUserDataResponses(data.Item2), data.Item3));
        }

        private IReadOnlyCollection<UserDataResponse> GetUserDataResponses(IReadOnlyCollection<UserData> userDatas)
        {
            List<UserDataResponse> userDataResponses = [];
            foreach(var userData in userDatas)
            {
                userDataResponses.Add(_userDataMapper.ToDTO(userData));
            }
            return userDataResponses;
        }
    }
}
