using AIChatServer.Entities.Chats;
using AIChatServer.Entities.User;
using AIChatServer.Managers.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Managers.Implementations.ChatStrategies.MatchStrategies
{
    public class GroupChatMatchStrategy(
        ILogger<GroupChatMatchStrategy> logger) : IChatMatchStrategy
    {
        private readonly ILogger<GroupChatMatchStrategy> _logger = logger ??
            throw new ArgumentNullException(nameof(logger));

        public Task MatchUserAsync(User user, IChatLifecycleManager chatCreator, string? chatPredicate = null)
        {
            _logger.LogInformation("Creating new group chat for user {UserId} with predicate {Predicate}",
                user.Id, chatPredicate ?? "default");

            try
            {
                chatCreator.CreateChat([user.Id], ChatType.Group);
                _logger.LogInformation("Successfully created group chat for user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create group chat for user {UserId}", user.Id);
                throw;
            }

            return Task.CompletedTask;
        }
    }
}