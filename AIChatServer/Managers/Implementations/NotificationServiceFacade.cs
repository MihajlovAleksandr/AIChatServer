using AIChatServer.Entities.Chats;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Service.Interfaces;
using AIChatServer.Utils.Interfaces;

namespace AIChatServer.Managers.Implementations
{
    public class NotificationServiceFacade
    {
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
            _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _chatManager = chatManager ?? throw new ArgumentNullException(nameof(chatManager));
        }

        public async Task SendChatNotificationAsync(Chat chat, string prompt)
        {
            var users = chat.UsersWithData.Keys.ToList();
            var tokens = _notificationService.GetNotificationTokens(users.ToArray());

            foreach (var userId in users)
            {
                if (!tokens.TryGetValue(userId, out var userTokens) || !chat.UsersWithData.TryGetValue(userId, out UserInChatData? data))
                    continue;

                foreach (var token in userTokens.Where(t => t != null))
                {
                    await _notificationManager.SendMessageToDevice(
                        token,
                        data.Name,
                        prompt,
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

            Chat chat = _chatManager.GetChat(message.Chat) ?? throw new ArgumentNullException(nameof(chat));

            foreach (var userId in users)
            {
                if (!allTokens.TryGetValue(userId, out var userTokens))
                    continue;

                chat.UsersWithData.TryGetValue(userId, out UserInChatData? data);
                if (data == null)
                    continue;

                foreach (var token in userTokens.Where(t => t != null))
                {
                    await _notificationManager.SendMessageToDevice(
                        token,
                        data.Name,
                        message.Text,
                        message.Chat,
                        false
                    );
                }

            }
        }
    }
}
