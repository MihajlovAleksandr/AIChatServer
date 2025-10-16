using AIChatServer.Entities.Connection.Interfaces;
using AIChatServer.Managers.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Managers.Implementations
{
    public class UserValidator(ITokenRefresher tokenRefresher, ILogger<UserValidator> logger) : IUserValidator
    {
        private readonly ITokenRefresher _tokenRefresher = tokenRefresher
            ?? throw new ArgumentNullException(nameof(tokenRefresher));
        private readonly ILogger<UserValidator> _logger = logger 
            ?? throw new ArgumentNullException(nameof(logger));

        public async Task<bool> ValidateAsync(Guid userId, bool needToRefreshToken, IConnection connection)
        {
            if (userId == default)
            {
                _logger.LogWarning("Validation failed: userId is default");
                return false;
            }

            if (needToRefreshToken)
            {
                _logger.LogInformation("Refreshing token for user {UserId}", userId);
                await _tokenRefresher.SendRefreshCommandAsync(connection, userId);
            }

            _logger.LogInformation("User {UserId} validated successfully", userId);
            return true;
        }
    }
}
