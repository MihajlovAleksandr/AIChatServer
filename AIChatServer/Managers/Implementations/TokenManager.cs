using AIChatServer.Managers.Interfaces;
using AIChatServer.Utils.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Managers.Implementations
{
    public class TokenManager : ITokenManager
    {
        private readonly IConnectionTokenManager _tokenManager;
        private readonly ILogger<TokenManager> _logger;

        public TokenManager(IConnectionTokenManager tokenManager, ILogger<TokenManager> logger)
        {
            _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string GenerateToken(Guid userId, Guid connectionId)
        {
            var token = _tokenManager.GenerateToken(userId, connectionId);
            _logger.LogInformation("Generated token for user {UserId} and connection {ConnectionId}.", userId, connectionId);
            return token;
        }

        public (Guid, Guid, bool) ParseToken(string? token)
        {
            if (token == null)
            {
                _logger.LogWarning("Attempted to parse null token.");
                return (default, default, default);
            }

            if (_tokenManager.ValidateToken(token, out Guid userId, out Guid connectionId, out DateTime expiration))
            {
                bool isExpiringSoon = expiration < DateTime.UtcNow.AddDays(7);
                _logger.LogInformation(
                    "Parsed token for user {UserId}, connection {ConnectionId}, expiring soon: {IsExpiringSoon}.",
                    userId,
                    connectionId,
                    isExpiringSoon
                );
                return (userId, connectionId, isExpiringSoon);
            }

            _logger.LogWarning("Failed to validate token: {Token}", token);
            return (default, default, default);
        }
    }
}
