using AIChatServer.Repositories.Interfaces;
using AIChatServer.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Service.Implementations
{
    public class NotificationService(INotificationRepository notificationRepository, ILogger<NotificationService> logger) : INotificationService
    {
        private readonly INotificationRepository _notificationRepository = notificationRepository 
            ?? throw new ArgumentNullException(nameof(notificationRepository));
        private readonly ILogger<NotificationService> _logger = logger 
            ?? throw new ArgumentNullException(nameof(logger));

        public bool UpdateNotifications(Guid userId, bool value)
        {
            _logger.LogInformation("Updating notifications for UserId: {UserId}, NewValue: {Value}", userId, value);
            try
            {
                var result = _notificationRepository.UpdateNotifications(userId, value);
                _logger.LogInformation("Notifications update completed for UserId: {UserId}, Result: {Result}", userId, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating notifications for UserId: {UserId}", userId);
                throw;
            }
        }

        public bool GetNotifications(Guid userId)
        {
            _logger.LogInformation("Fetching notifications state for UserId: {UserId}", userId);
            try
            {
                var result = _notificationRepository.GetNotifications(userId);
                _logger.LogInformation("Fetched notifications state for UserId: {UserId}, Enabled: {Result}", userId, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching notifications for UserId: {UserId}", userId);
                throw;
            }
        }

        public bool UpdateNotificationToken(Guid id, string token)
        {
            ArgumentNullException.ThrowIfNull(token);

            _logger.LogInformation("Updating notification token for UserId: {UserId}", id);
            try
            {
                var result = _notificationRepository.UpdateNotificationToken(id, token);
                _logger.LogInformation("Notification token updated for UserId: {UserId}, Result: {Result}", id, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating notification token for UserId: {UserId}", id);
                throw;
            }
        }

        public Dictionary<Guid, List<string>> GetNotificationTokens(Guid[] userIds)
        {
            ArgumentNullException.ThrowIfNull(userIds);

            if (userIds.Length == 0)
            {
                _logger.LogWarning("Empty userIds array passed to GetNotificationTokens.");
                return new Dictionary<Guid, List<string>>();
            }

            _logger.LogInformation("Fetching notification tokens for {Count} users", userIds.Length);
            try
            {
                var result = _notificationRepository.GetNotificationTokens(userIds);
                _logger.LogInformation("Fetched notification tokens for {Count} users", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching notification tokens for multiple users.");
                throw;
            }
        }

        public List<string> GetNotificationTokens(Guid userId)
        {
            _logger.LogInformation("Fetching notification tokens for UserId: {UserId}", userId);
            try
            {
                var result = _notificationRepository.GetNotificationTokens(new[] { userId });

                if (result.TryGetValue(userId, out var tokens))
                {
                    _logger.LogInformation("Fetched {Count} tokens for UserId: {UserId}", tokens.Count, userId);
                    return tokens;
                }

                _logger.LogWarning("No tokens found for UserId: {UserId}", userId);
                return new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching notification tokens for UserId: {UserId}", userId);
                throw;
            }
        }
    }
}
