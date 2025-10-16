using AIChatServer.Repositories.Constants;
using AIChatServer.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AIChatServer.Repositories.Implementations
{
    public class NotificationRepository(string connectionString, 
        ILogger<NotificationRepository> logger) : BaseRepository(connectionString), INotificationRepository
    {
        private readonly ILogger<NotificationRepository> _logger = logger 
            ?? throw new ArgumentNullException(nameof(logger));

        public bool UpdateNotifications(Guid userId, bool value)
        {
            try
            {
                _logger.LogInformation("Updating email notification setting for UserId={UserId} to {Value}", userId, value);

                using var connection = GetConnection();
                using var command = new NpgsqlCommand(NotificationQueries.UpdateNotifications, connection);
                command.Parameters.AddWithValue("@EmailNotifications", value);
                command.Parameters.AddWithValue("@UserId", userId);

                bool updated = command.ExecuteNonQuery() > 0;
                _logger.LogInformation("Email notification setting updated for UserId={UserId}: {Updated}", userId, updated);
                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update email notifications for UserId={UserId}", userId);
                throw;
            }
        }

        public bool GetNotifications(Guid userId)
        {
            try
            {
                _logger.LogInformation("Retrieving email notification setting for UserId={UserId}", userId);

                using var connection = GetConnection();
                using var command = new NpgsqlCommand(NotificationQueries.GetNotifications, connection);
                command.Parameters.AddWithValue("@UserId", userId);

                object result = command.ExecuteScalar();
                bool enabled = result != null && result != DBNull.Value && Convert.ToBoolean(result);

                _logger.LogInformation("Email notification setting for UserId={UserId}: {Enabled}", userId, enabled);
                return enabled;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve email notifications for UserId={UserId}", userId);
                throw;
            }
        }

        public bool UpdateNotificationToken(Guid id, string token)
        {
            try
            {
                _logger.LogInformation("Updating notification token for ConnectionId={ConnectionId}", id);

                using var connection = GetConnection();
                using var command = new NpgsqlCommand(NotificationQueries.UpdateNotificationToken, connection);
                command.Parameters.AddWithValue("@NotificationToken", token);
                command.Parameters.AddWithValue("@Id", id);

                bool updated = command.ExecuteNonQuery() > 0;
                _logger.LogInformation("Notification token updated for ConnectionId={ConnectionId}: {Updated}", id, updated);
                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update notification token for ConnectionId={ConnectionId}", id);
                throw;
            }
        }

        public Dictionary<Guid, List<string>> GetNotificationTokens(Guid[] userIds)
        {
            var result = new Dictionary<Guid, List<string>>();

            if (userIds == null || userIds.Length == 0)
            {
                _logger.LogInformation("No userIds provided to retrieve notification tokens.");
                return result;
            }

            try
            {
                _logger.LogInformation("Retrieving notification tokens for {UserCount} users", userIds.Length);

                using var connection = GetConnection();
                using var command = new NpgsqlCommand(NotificationQueries.GetNotificationTokens, connection);

                command.Parameters.AddWithValue("@ids", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Uuid, userIds);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Guid userId = reader.GetGuid(0);

                    if (!reader.IsDBNull(1))
                    {
                        string token = reader.GetString(1);

                        if (!result.ContainsKey(userId))
                            result[userId] = new List<string>();

                        result[userId].Add(token);
                    }
                }

                foreach (var userId in userIds)
                {
                    if (!result.ContainsKey(userId))
                        result[userId] = new List<string>();
                }

                _logger.LogInformation("Retrieved notification tokens for {UserCount} users", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve notification tokens for users");
                throw;
            }
        }
    }
}
