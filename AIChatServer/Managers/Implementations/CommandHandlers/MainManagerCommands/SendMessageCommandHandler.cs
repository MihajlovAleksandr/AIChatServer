using AIChatServer.Entities.Connection;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Service.Interfaces;
using AIChatServer.Utils;
using AIChatServer.Entities.Chats;
using AIChatServer.Utils.Interfaces;
using AIChatServer.Entities.DTO.Request;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Utils.Interfaces.Mapper;

namespace AIChatServer.Managers.Implementations.CommandHandlers.MainManagerCommands
{
    public class SendMessageCommandHandler(IChatService chatService, IChatManager chatManager,
        IUserManager userManager, IAIManager aiManager, INotificationService notificationService,
        INotificationManager notificationManager, ISerializer serializer,
        IMapper<MessageRequest, Message, MessageResponse> messageMapper, 
        ISendCommandMapper sendCommandMapper) : ICommandHandler
    {
        private readonly IChatService _chatService = chatService
            ?? throw new ArgumentNullException(nameof(chatService));
        private readonly IChatManager _chatManager = chatManager
            ?? throw new ArgumentNullException(nameof(chatManager));
        private readonly IUserManager _userManager = userManager
            ?? throw new ArgumentNullException(nameof(userManager));
        private readonly IAIManager _aiManager = aiManager
            ?? throw new ArgumentNullException(nameof(aiManager));
        private readonly INotificationService _notificationService = notificationService
            ?? throw new ArgumentNullException(nameof(notificationService));
        private readonly INotificationManager _notificationManager = notificationManager
            ?? throw new ArgumentNullException(nameof(notificationManager));
        private readonly ISerializer _serializer = serializer
            ?? throw new ArgumentNullException(nameof(serializer));
        private readonly IMapper<MessageRequest, Message, MessageResponse> _messageMapper = messageMapper
            ?? throw new ArgumentNullException(nameof(messageMapper));
        private readonly ISendCommandMapper _sendCommandMapper = sendCommandMapper
            ?? throw new ArgumentNullException(nameof(sendCommandMapper));

        public string Operation => "SendMessage";

        public async Task HandleAsync(object sender, Command command)
        {
            var knownUser = (IServerUser)sender;
            MessageRequest messageRequest = command.GetData<MessageRequest>() ?? throw new ArgumentNullException("MessageRequest");

            if (messageRequest.Sender != knownUser.User.Id)
            {
                await CommandSender.SendCommandAsync(command.Sender, new CommandResponse("Logout"), _serializer);
                return;
            }

            Message message = _messageMapper.ToModel(messageRequest);
            message = _chatService.SendMessage(message);
            List<Guid> users = _chatManager.GetUsersInChat(message.Chat);

            await _userManager.SendCommandAsync(_sendCommandMapper.MapToSendCommand(users, new CommandResponse("SendMessage", _messageMapper.ToDTO(message))));

            users.Remove(message.Sender);
            await SendMessageNotification(users, message);

            if (users.Contains(_aiManager.AIId))
            {
                await _aiManager.SendMessageAsync(message.Chat, message.Text);
            }
        }

        private async Task SendMessageNotification(List<Guid> users, Message message)
        {
            var allTokens = _notificationService.GetNotificationTokens(users.ToArray());

            for (int i = 0; i < users.Count; i++)
            {
                if (allTokens.TryGetValue(users[i], out List<string>? userTokens))
                {
                    string? name = _chatManager.GetChatName(message.Chat, users[i]);
                    if (name != null)
                    {
                        for (int j = 0; j < userTokens.Count; j++)
                        {
                            string token = userTokens[j];
                            if (token != null)
                            {
                                await _notificationManager.SendMessageToDevice(token, name, message.Text, message.Chat, false);
                            }
                        }
                    }
                }
            }
        }
    }
}
