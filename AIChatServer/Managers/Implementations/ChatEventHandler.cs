using AIChatServer.Entities.Chats;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Utils.Interfaces.Mapper;

namespace AIChatServer.Managers.Implementations
{
    public class ChatEventHandler : IEventHandler
    {
        private readonly IChatManager _chatManager;
        private readonly IAIManager _aiManager;
        private readonly NotificationServiceFacade _notifications;
        private readonly IResponseMapper<ChatResponse, ChatWithUserContext> _chatMapper;

        public ChatEventHandler(
            IChatManager chatManager,
            IAIManager aiManager,
            NotificationServiceFacade notifications,
            IResponseMapper<ChatResponse, ChatWithUserContext> chatMapper)
        {
            _chatManager = chatManager;
            _aiManager = aiManager;
            _notifications = notifications;
            _chatMapper = chatMapper;
        }

        public void Subscribe()
        {
            _chatManager.OnChatCreated += async chat => await HandleChatCreated(chat);
            _chatManager.OnChatEnded += async chat => await HandleChatEnded(chat);
        }

        public Task HandleAsync(object? sender, object? args) => Task.CompletedTask;

        private async Task HandleChatCreated(Chat chat)
        {
            if (chat.ContainsAI(_aiManager.AIId))
                _aiManager.CreateDialog(chat.Id);

            await _notifications.SendChatCommandAsync("CreateChat", chat, _chatMapper);
            await _notifications.SendChatNotificationAsync(chat);
        }

        private async Task HandleChatEnded(Chat chat)
        {
            if (chat.ContainsAI(_aiManager.AIId))
                _aiManager.EndDialog(chat.Id);

            await _notifications.SendChatCommandAsync("EndChat", chat, _chatMapper);
            await _notifications.SendChatNotificationAsync(chat);
        }
    }
}
