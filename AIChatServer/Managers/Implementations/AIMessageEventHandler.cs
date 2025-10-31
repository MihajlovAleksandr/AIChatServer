using AIChatServer.Entities.Chats;
using AIChatServer.Entities.DTO.Request;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Service.Interfaces;
using AIChatServer.Utils.Interfaces;
using AIChatServer.Utils.Interfaces.Mapper;
using System.Management;

namespace AIChatServer.Managers.Implementations
{
    public class AIMessageEventHandler : IEventHandler
    {
        private readonly IAIManager _aiManager;
        private readonly IMessageService _messageService;
        private readonly IChatManager _chatManager;
        private readonly IUserManager _userManager;
        private readonly ISendCommandMapper _commandMapper;
        private readonly IMapper<MessageRequest, Message, MessageResponse> _messageMapper;
        private readonly NotificationServiceFacade _notifications;

        public AIMessageEventHandler(
            IAIManager aiManager,
            IMessageService messageService,
            IChatManager chatManager,
            IUserManager userManager,
            ISendCommandMapper commandMapper,
            IMapper<MessageRequest, Message, MessageResponse> messageMapper,
            NotificationServiceFacade notifications)
        {
            _aiManager = aiManager;
            _messageService = messageService;
            _chatManager = chatManager;
            _userManager = userManager;
            _commandMapper = commandMapper;
            _messageMapper = messageMapper;
            _notifications = notifications;
        }

        public void Subscribe() => _aiManager.OnSendMessage += async m => await HandleAsync(this, m);

        public async Task HandleAsync(object? sender, object? args)
        {
            if (args is not Message message)
                throw new ArgumentException(nameof(args));

            var savedMessage = _messageService.SendMessage(message);
            Chat chat = _chatManager.GetChat(savedMessage.Chat) ?? throw new ArgumentNullException(nameof(chat));
            var users = chat.UsersWithData.Keys.ToList();

            await _userManager.SendCommandAsync(_commandMapper.MapToSendCommand(
                users, new CommandResponse("SendMessage", _messageMapper.ToDTO(savedMessage)!)));

            users.Remove(savedMessage.Sender);
            await _notifications.SendMessageNotificationAsync(users, savedMessage);

            if (users.Contains(_aiManager.AIId))
                await _aiManager.SendMessageAsync(savedMessage.Chat, savedMessage.Text);
        }
    }
}
