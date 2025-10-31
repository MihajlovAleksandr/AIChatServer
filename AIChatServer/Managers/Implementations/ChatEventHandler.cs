using AIChatServer.Entities.Chats;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Entities.User;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Utils.Implementations.Mappers;
using AIChatServer.Utils.Interfaces.Mapper;

namespace AIChatServer.Managers.Implementations
{
    public class ChatEventHandler : IEventHandler
    {
        private readonly IChatManager _chatManager;
        private readonly IAIManager _aiManager;
        private readonly NotificationServiceFacade _notifications;
        private readonly CommandServiceFacade _commands;
        private readonly IResponseMapper<ChatResponse, ChatWithUserContext> _chatMapper;
        private readonly IResponseMapper<UserDataResponse, UserData> _userDataMapper;

        public ChatEventHandler(
            IChatManager chatManager,
            IAIManager aiManager,
            NotificationServiceFacade notifications,
            CommandServiceFacade commands,
            IResponseMapper<ChatResponse, ChatWithUserContext> chatMapper,
            IResponseMapper<UserDataResponse, UserData> userDataMapper)
        {
            _chatManager = chatManager;
            _aiManager = aiManager;
            _notifications = notifications;
            _commands = commands;
            _chatMapper = chatMapper;
            _userDataMapper = userDataMapper;
        }

        public void Subscribe()
        {
            _chatManager.OnChatCreated += async chat => await HandleChatCreated(chat);
            _chatManager.OnChatEnded += async chat => await HandleChatEnded(chat);
            _chatManager.OnUserAdded += async (user, chat) => await HandleUserAdded(user, chat);
            _chatManager.OnUserRemoved += async (userId, chat) => await HandleUserRemoved(userId, chat);
        }

        public Task HandleAsync(object? sender, object? args) => Task.CompletedTask;

        private async Task HandleChatCreated(Chat chat)
        {
            if (chat.ContainsAI(_aiManager.AIId))
                _aiManager.CreateDialog(chat.Id);
            await _commands.SendChatCommandAsync("CreateChat", chat, _chatMapper);
            await _notifications.SendChatNotificationAsync(chat, "new_chat");
        }

        private async Task HandleChatEnded(Chat chat)
        {
            if (chat.ContainsAI(_aiManager.AIId))
                _aiManager.EndDialog(chat.Id);

            await _commands.SendChatCommandAsync("EndChat", chat, _chatMapper);
            await _notifications.SendChatNotificationAsync(chat, "end_chat");
        }

        private async Task HandleUserAdded(User user, Chat chat)
        {
            await _commands.SendUserDataCommandAsync(user.UserData, user.Id, chat, _userDataMapper, _chatMapper);
            await _notifications.SendChatNotificationAsync(chat, "add_user");   
        }

        private async Task HandleUserRemoved(Guid userId, Chat chat)
        {
            await _commands.SendUserRemoveCommandAsync(userId, chat);
            await _notifications.SendChatNotificationAsync(chat, "remove_user");
        }
    }
}
