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
        private readonly Func<Guid, bool> _isChatSearching;
        private readonly ICollectionResponseMapper<MessageResponse, Message> _messageMapper;
        private readonly ICollectionResponseMapper<ChatResponse, ChatWithUserContext> _chatMapper;
        private readonly ILogger<SyncManager> _logger;

        public SyncManager(
            IChatService chatService,
            Func<Guid, bool> isChatSearching,
            ICollectionResponseMapper<MessageResponse, Message> messageMapper,
            ICollectionResponseMapper<ChatResponse, ChatWithUserContext> chatMapper,
            ILogger<SyncManager> logger)
        {
            _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
            _isChatSearching = isChatSearching ?? throw new ArgumentNullException(nameof(isChatSearching));
            _messageMapper = messageMapper ?? throw new ArgumentNullException(nameof(messageMapper));
            _chatMapper = chatMapper ?? throw new ArgumentNullException(nameof(chatMapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public CommandResponse GetSyncCommand(Guid userId, DateTime lastOnline)
        {
            _logger.LogInformation("Generating sync command for user {UserId} since {LastOnline}.", userId, lastOnline);

            var chats = _chatService.GetNewChats(userId, lastOnline);
            var messages = _chatService.GetNewMessages(userId, lastOnline);

            var messageResponsesMain = _messageMapper.ToDTO(messages.Item1);
            var messageResponsesCompressed = _messageMapper.ToDTO(messages.Item2);

            var newChats = _chatMapper.ToDTO(chats.Item1.Select(x => new ChatWithUserContext(x, userId)));
            var updatedChats = _chatMapper.ToDTO(chats.Item2.Select(x => new ChatWithUserContext(x, userId)));

            bool searching = _isChatSearching(userId);

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
                new SyncDBResponse(messageResponsesMain, messageResponsesCompressed, newChats, updatedChats, searching)
            );
        }
    }
}
