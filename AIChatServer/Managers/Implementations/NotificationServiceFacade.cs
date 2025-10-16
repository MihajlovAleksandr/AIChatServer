using AIChatServer.Entities.Chats;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Service.Interfaces;
using AIChatServer.Utils.Interfaces;
using AIChatServer.Utils.Interfaces.Mapper;

namespace AIChatServer.Managers.Implementations
{
    public class NotificationServiceFacade
    {
        private readonly IUserManager _userManager;
        private readonly INotificationManager _notificationManager;
        private readonly INotificationService _notificationService;
        private readonly IChatManager _chatManager;

        public NotificationServiceFacade(
            IUserManager userManager,
            INotificationManager notificationManager,
            INotificationService notificationService,
            ISendCommandMapper sendCommandMapper,
            IChatManager chatManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _chatManager = chatManager ?? throw new ArgumentNullException(nameof(chatManager));
        }

        public async Task SendChatCommandAsync(
            string commandName,
            Chat chat,
            IResponseMapper<ChatResponse, ChatWithUserContext> mapper)
        {
            var userCommands = new Dictionary<Guid, CommandResponse>();

            foreach (var (userId, _) in chat.UsersNames)
            {
                var context = new ChatWithUserContext(chat, userId);
                var dto = mapper.ToDTO(context) ?? throw new InvalidOperationException("ChatResponse mapping failed");
                userCommands[userId] = new CommandResponse(commandName, dto);
            }

            await _userManager.SendCommandAsync(userCommands);
        }

        public async Task SendChatNotificationAsync(Chat chat)
        {
            var users = chat.UsersNames.Keys.ToList();
            var tokens = _notificationService.GetNotificationTokens(users.ToArray());

            foreach (var userId in users)
            {
                if (!tokens.TryGetValue(userId, out var userTokens) || !chat.UsersNames.TryGetValue(userId, out var name))
                    continue;

                foreach (var token in userTokens.Where(t => t != null))
                {
                    var isNewChat = chat.EndTime == null;
                    var notificationType = isNewChat ? "new_chat" : "end_chat";

                    await _notificationManager.SendMessageToDevice(
                        token,
                        name,
                        notificationType,
                        chat.Id,
                        true
                    );
                }
            }
        }

        public async Task SendMessageNotificationAsync(List<Guid> users, Message message)
        {
            if (users == null || users.Count == 0)
                return;

            var allTokens = _notificationService.GetNotificationTokens(users.ToArray());

            foreach (var userId in users)
            {
                if (!allTokens.TryGetValue(userId, out var userTokens))
                    continue;

                var chatName = _chatManager.GetChatName(message.Chat, userId);
                if (chatName == null)
                    continue;

                foreach (var token in userTokens.Where(t => t != null))
                {
                    await _notificationManager.SendMessageToDevice(
                        token,
                        chatName,
                        message.Text,
                        message.Chat,
                        false
                    );
                }
            }
        }
    }
}
