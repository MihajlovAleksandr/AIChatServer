using AIChatServer.Entities.User;
using AIChatServer.Entities.Chats;
using Microsoft.Extensions.Logging;
using AIChatServer.Managers.Interfaces;

namespace AIChatServer.Managers.Implementations.ChatStrategies.MatchStrategies
{
    public class AIChatMatchStrategy(Guid aiId,
            ILogger<AIChatMatchStrategy> logger) : IChatMatchStrategy
    {
        private readonly Guid _aiId = aiId;
        private readonly ILogger<AIChatMatchStrategy> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        public Task MatchUserAsync(User user, IChatLifecycleManager chatCreator, string? predicate)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            _logger.LogInformation("Matching User {UserId} with AI {AIId}", user.Id, _aiId);
            try
            {
                chatCreator.CreateChat(new[] { user.Id, _aiId }, ChatType.AI);
                _logger.LogInformation("Successfully matched User {UserId} with AI {AIId}", user.Id, _aiId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to match User {UserId} with AI {AIId}", user.Id, _aiId);
                throw;
            }

            return Task.CompletedTask;
        }
    }
}
