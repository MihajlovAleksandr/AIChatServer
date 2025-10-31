using AIChatServer.Entities.Chats;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Service.Interfaces;
using AIChatServer.Utils.Interfaces.Mapper;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Managers.Implementations
{
    public class SyncManager : ISyncManager
    {
        private readonly IChatService _chatService;
        private readonly IMessageService _messageService;
        private readonly Func<Guid, bool> _isChatSearching;
        private readonly Func<Guid, Guid?> _isUserAdding;
        private readonly ICollectionResponseMapper<MessageResponse, Message> _messageMapper;
        private readonly ICollectionResponseMapper<ChatResponse, ChatWithUserContext> _chatMapper;
        private readonly ILogger<SyncManager> _logger;

        public SyncManager(
            IChatService chatService,
            IMessageService messageService,
            Func<Guid, bool> isChatSearching,
            Func<Guid, Guid?> isUserAdding,
            ICollectionResponseMapper<MessageResponse, Message> messageMapper,
            ICollectionResponseMapper<ChatResponse, ChatWithUserContext> chatMapper,
            ILogger<SyncManager> logger)
        {
            _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
            _isChatSearching = isChatSearching ?? throw new ArgumentNullException(nameof(isChatSearching));
            _isUserAdding = isUserAdding ?? throw new ArgumentNullException(nameof(isUserAdding));
            _messageMapper = messageMapper ?? throw new ArgumentNullException(nameof(messageMapper));
            _chatMapper = chatMapper ?? throw new ArgumentNullException(nameof(chatMapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public CommandResponse GetSyncCommand(Guid userId, DateTime lastOnline)
        {
            _logger.LogInformation("Generating sync command for user {UserId} since {LastOnline}.", userId, lastOnline);

            var chats = _chatService.GetNewChats(userId, lastOnline);
            var messages = _messageService.GetNewMessages(userId, lastOnline);

            var messageResponsesMain = _messageMapper.ToDTO(messages.Item1);
            var messageResponsesCompressed = _messageMapper.ToDTO(messages.Item2);

            var newChats = _chatMapper.ToDTO(chats.Item1.Select(x => new ChatWithUserContext(x, userId)));
            var updatedChats = _chatMapper.ToDTO(chats.Item2.Select(x => new ChatWithUserContext(x, userId)));

            bool searching = _isChatSearching(userId);
            Guid? userAddingToChat = _isUserAdding(userId);

            _logger.LogInformation(
                "Sync for user {UserId}: {NewMessages} new messages, {UpdatedMessages} updated messages, {NewChats} new chats, {UpdatedChats} updated chats, Searching={Searching}.",
                userId,
                messageResponsesMain.Count,
                messageResponsesCompressed.Count,
                newChats.Count,
                updatedChats.Count,
                searching
            );

            return new CommandResponse(
                "SyncDB",
                new SyncDBResponse(messageResponsesMain, messageResponsesCompressed, newChats,
                updatedChats, searching, userAddingToChat)
            );
        }
    }
}
