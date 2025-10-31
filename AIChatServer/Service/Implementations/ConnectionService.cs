using AIChatServer.Entities.Connection;
using AIChatServer.Repositories.Interfaces;
using AIChatServer.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Service.Implementations
{
    public class ConnectionService(IConnectionRepository connectionRepository, ILogger<ConnectionService> logger) : IConnectionService
    {
        private readonly IConnectionRepository _connectionRepository = connectionRepository 
            ?? throw new ArgumentNullException(nameof(connectionRepository));
        private readonly ILogger<ConnectionService> _logger = logger 
            ?? throw new ArgumentNullException(nameof(logger));

        public Guid AddConnection(string device)
        {
            if (string.IsNullOrEmpty(device))
            {
                _logger.LogWarning("Attempt to add connection with empty device name.");
                throw new ArgumentException("Device cannot be null or empty", nameof(device));
            }

            var connectionId = _connectionRepository.AddConnection(device);
            _logger.LogInformation("Connection {ConnectionId} added for device {Device}.", connectionId, device);

            return connectionId;
        }

        public ConnectionInfo GetConnectionInfo(Guid connectionId, Guid defaultUserId = default)
        {
            var info = _connectionRepository.GetConnectionInfo(connectionId, defaultUserId);

            if (info == null)
            {
                _logger.LogWarning("Connection info not found for ConnectionId {ConnectionId}.", connectionId);
            }
            else
            {
                _logger.LogInformation("Retrieved connection info for ConnectionId {ConnectionId}.", connectionId);
            }

            return info;
        }

        public List<ConnectionInfo> GetAllUserConnections(Guid userId)
        {
            var connections = _connectionRepository.GetAllUserConnections(userId);
            _logger.LogInformation("Retrieved {Count} connections for User {UserId}.", connections.Count, userId);
            return connections;
        }

        public bool VerifyConnection(Guid id, Guid userId, string device, out DateTime lastConnection)
        {
            if (string.IsNullOrEmpty(device))
            {
                _logger.LogWarning("Attempt to verify connection with empty device name for User {UserId}.", userId);
                throw new ArgumentException("Device cannot be null or empty", nameof(device));
            }

            var isValid = _connectionRepository.VerifyConnection(id, userId, device, out lastConnection);

            if (isValid)
            {
                _logger.LogInformation("Connection {ConnectionId} for User {UserId} on Device {Device} verified. Last connection: {LastConnection}.", id, userId, device, lastConnection);
            }
            else
            {
                _logger.LogWarning("Failed to verify Connection {ConnectionId} for User {UserId} on Device {Device}.", id, userId, device);
            }

            return isValid;
        }

        public ConnectionInfo RemoveConnection(Guid id)
        {
            var removed = _connectionRepository.RemoveConnection(id);

            if (removed == null)
            {
                _logger.LogWarning("Attempted to remove non-existing Connection {ConnectionId}.", id);
            }
            else
            {
                _logger.LogInformation("Connection {ConnectionId} removed successfully.", id);
            }

            return removed;
        }

        public bool SetLastConnection(Guid connectionId, bool isOnline)
        {
            var result = _connectionRepository.SetLastConnection(connectionId, isOnline);

            if (result)
            {
                _logger.LogInformation("Updated last connection status for Connection {ConnectionId} to {Status}.", connectionId, isOnline ? "Online" : "Offline");
            }
            else
            {
                _logger.LogWarning("Failed to update last connection status for Connection {ConnectionId}.", connectionId);
            }

            return result;
        }

        public int[] GetConnectionCount(Guid userId)
        {
            var count = _connectionRepository.GetConnectionCount(userId);
            _logger.LogInformation("Retrieved connection count for User {UserId}: {Active} active, {Inactive} inactive.", userId, count[0], count[1]);
            return count;
        }

        public void DeleteUnknownConnection(Guid id)
        {
            _connectionRepository.DeleteUnknownConnection(id);
            _logger.LogInformation("Deleted unknown Connection {ConnectionId}.", id);
        }

        public void UpdateConnection(Guid connectionId, Guid userId)
        {
            _connectionRepository.UpdateConnection(connectionId, userId);
            _logger.LogInformation("Updated Connection {ConnectionId} to belong to User {UserId}.", connectionId, userId);
        }

        public DateTime? GetLastUserOnline(Guid userId)
        {
            ArgumentNullException.ThrowIfNull(userId);
            var lastOnline = _connectionRepository.GetLastUserOnline(userId);
            _logger.LogInformation("Get last user {userId} online: {userOnline}", userId, lastOnline);
            return lastOnline;
        }
    }
}
