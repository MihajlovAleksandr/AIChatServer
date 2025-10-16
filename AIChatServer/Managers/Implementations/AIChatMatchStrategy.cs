using AIChatServer.Entities.User;
using AIChatServer.Entities.Chats;
using AIChatServer.Managers.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Managers.Implementations
{
    public class AIChatMatchStrategy : IChatMatchStrategy
    {
        private readonly Guid _aiId;
        private readonly ILogger<AIChatMatchStrategy> _logger;

        public AIChatMatchStrategy(Guid aiId, ILogger<AIChatMatchStrategy> logger)
        {
            _aiId = aiId;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task MatchUserAsync(User user, IChatCreator chatCreator, ChatType? chatType )
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (chatCreator == null) throw new ArgumentNullException(nameof(chatCreator));
            ChatType currentChatType = chatType ?? ChatType.AI;

            _logger.LogInformation("Matching User {UserId} with AI {AIId}", user.Id, _aiId);
            try
            {
                chatCreator.CreateChat(new[] { user.Id, _aiId }, currentChatType);
                _logger.LogInformation("Successfully matched User {UserId} with AI {AIId}", user.Id, _aiId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to match User {UserId} with AI {AIId}", user.Id, _aiId);
                throw;
            }

            await Task.CompletedTask;
        }
    }
}
