using AIChatServer.Entities.Connection;
using AIChatServer.Entities.Exceptions;
using AIChatServer.Repositories.Constants;
using AIChatServer.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;

namespace AIChatServer.Repositories.Implementations
{
    public class ConnectionRepository(string connectionString, IAuthRepository authRepository,
        ILogger<ConnectionRepository> logger) : BaseRepository(connectionString), IConnectionRepository
    {
        private readonly ILogger<ConnectionRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IAuthRepository _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));

        public Guid AddConnection(string device)
        {
            try
            {
                _logger.LogInformation("Adding new connection for device: {Device}", device);

                using var connection = GetConnection();
                using var command = new NpgsqlCommand(ConnectionQueries.AddConnection, connection);
                command.Parameters.AddWithValue("@Device", device);

                var id = (Guid)command.ExecuteScalar();
                _logger.LogInformation("New connection added with Id={ConnectionId}", id);
                return id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add connection for device: {Device}", device);
                throw;
            }
        }

        public ConnectionInfo GetConnectionInfo(Guid connectionId, Guid defaultUserId = default)
        {
            try
            {
                _logger.LogInformation("Retrieving connection info for ConnectionId={ConnectionId}", connectionId);

                using var connection = GetConnection();
                using var command = new NpgsqlCommand(ConnectionQueries.GetConnectionInfo, connection);
                command.Parameters.AddWithValue("@ConnectionId", connectionId);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var info = new ConnectionInfo(
                        reader.GetGuid("id"),
                        reader.IsDBNull("user_id") ? defaultUserId : reader.GetGuid("user_id"),
                        reader.GetString("device"),
                        reader.IsDBNull("last_connection") ? null : reader.GetDateTime("last_connection")
                    );

                    _logger.LogInformation("Retrieved connection info for ConnectionId={ConnectionId}", connectionId);
                    return info;
                }

                _logger.LogWarning("ConnectionId={ConnectionId} not found", connectionId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve connection info for ConnectionId={ConnectionId}", connectionId);
                throw;
            }
        }

        public List<ConnectionInfo> GetAllUserConnections(Guid userId)
        {
            try
            {
                _logger.LogInformation("Retrieving all connections for UserId={UserId}", userId);

                var connections = new List<ConnectionInfo>();
                using var connection = GetConnection();
                using var command = new NpgsqlCommand(ConnectionQueries.GetAllUserConnections, connection);
                command.Parameters.AddWithValue("@UserId", userId);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    connections.Add(new ConnectionInfo(
                        reader.GetGuid("id"),
                        reader.GetGuid("user_id"),
                        reader.GetString("device"),
                        reader.IsDBNull("last_connection") ? null : reader.GetDateTime("last_connection")
                    ));
                }

                _logger.LogInformation("Retrieved {ConnectionCount} connections for UserId={UserId}", connections.Count, userId);
                return connections;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve connections for UserId={UserId}", userId);
                throw;
            }
        }

        public bool VerifyConnection(Guid id, Guid userId, string device, out DateTime lastConnection)
        {
            try
            {
                _logger.LogInformation("Verifying connection Id={ConnectionId} for UserId={UserId} and device={Device}", id, userId, device);
                var userBan = _authRepository.GetUserBanById(userId);
                if (userBan != null && userBan.BannedUntil > DateTime.UtcNow)
                {
                    _logger.LogWarning("UserId={UserId} is banned until {Until}. Blocking connection.", userId, userBan.BannedUntil);
                    throw new UserBannedException(userBan);
                }

                using var connection = GetConnection();
                using var command = new NpgsqlCommand(ConnectionQueries.VerifyConnection, connection);
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@Device", device);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    lastConnection = reader.IsDBNull(0) ? DateTime.MinValue : reader.GetDateTime(0);
                    long count = reader.GetInt64(1);
                    bool verified = count == 1;

                    _logger.LogInformation("Connection verification for Id={ConnectionId}: {Verified}", id, verified);
                    return verified;
                }

                lastConnection = DateTime.MinValue;
                _logger.LogWarning("Connection Id={ConnectionId} not found for verification", id);
                return false;
            }
            catch (UserBannedException userBannedEx)
            {
                _logger.LogWarning("Banned user {UserId} tried to connect: {Reason}", userBannedEx.UserBan.UserId, userBannedEx.UserBan.Reason);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to verify connection Id={ConnectionId}", id);
                lastConnection = DateTime.MinValue;
                throw;
            }
        }

        public ConnectionInfo RemoveConnection(Guid id)
        {
            try
            {
                var connectionInfo = GetConnectionInfo(id);
                if (connectionInfo != null)
                {
                    using var connection = GetConnection();
                    using var command = new NpgsqlCommand(ConnectionQueries.RemoveConnection, connection);
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();

                    _logger.LogInformation("Removed connection association for Id={ConnectionId}", id);
                }
                else
                {
                    _logger.LogWarning("No connection found to remove for Id={ConnectionId}", id);
                }

                return connectionInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove connection Id={ConnectionId}", id);
                throw;
            }
        }

        public bool SetLastConnection(Guid connectionId, bool isOnline)
        {
            string query = isOnline
                ? ConnectionQueries.SetLastConnectionOnline
                : ConnectionQueries.SetLastConnectionOffline;

            try
            {
                using var connection = GetConnection();
                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@ConnectionId", connectionId);

                bool success = command.ExecuteNonQuery() > 0;
                _logger.LogInformation("Set last connection for Id={ConnectionId} to {Status}", connectionId, isOnline ? "online" : "offline");
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set last connection for Id={ConnectionId}", connectionId);
                throw;
            }
        }

        public int[] GetConnectionCount(Guid userId)
        {
            try
            {
                using var connection = GetConnection();
                using var command = new NpgsqlCommand(ConnectionQueries.GetConnectionCount, connection);
                command.Parameters.AddWithValue("@UserId", userId);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    int[] counts = [(int)reader.GetInt64("devices_count"), (int)reader.GetInt64("online_devices_count")];
                    _logger.LogInformation("Retrieved connection counts for UserId={UserId}: Total={Total}, Online={Online}", userId, counts[0], counts[1]);
                    return counts;
                }

                return [-1, -1];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve connection counts for UserId={UserId}", userId);
                throw;
            }
        }

        public void DeleteUnknownConnection(Guid id)
        {
            try
            {
                using var connection = GetConnection();
                using var command = new NpgsqlCommand(ConnectionQueries.DeleteUnknownConnection, connection);
                command.Parameters.AddWithValue("@Id", id);
                command.ExecuteNonQuery();

                _logger.LogInformation("Deleted unknown connection Id={ConnectionId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete unknown connection Id={ConnectionId}", id);
                throw;
            }
        }

        public void UpdateConnection(Guid connectionId, Guid userId)
        {
            try
            {
                using var connection = GetConnection();
                using var command = new NpgsqlCommand(ConnectionQueries.UpdateConnection, connection);
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@Id", connectionId);
                command.ExecuteNonQuery();

                _logger.LogInformation("Updated connection Id={ConnectionId} to UserId={UserId}", connectionId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update connection Id={ConnectionId} to UserId={UserId}", connectionId, userId);
                throw;
            }
        }

        public DateTime? GetLastUserOnline(Guid userId)
        {
            try
            {
                using var connection = GetConnection();
                using var command = new NpgsqlCommand(ConnectionQueries.GetLastUserOnline, connection);
                command.Parameters.AddWithValue("@UserId", userId);

                var result = command.ExecuteScalar();

                if (result == DBNull.Value || result is null)
                {
                    _logger.LogInformation("User {UserId} is currently online (LastOnline = null)", userId);
                    return null;
                }

                var lastOnline = (DateTime)result;
                _logger.LogInformation("User {UserId} last online at {LastOnline}", userId, lastOnline);

                return lastOnline;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get last online time for user {UserId}", userId);
                throw;
            }
        }
    }
}
