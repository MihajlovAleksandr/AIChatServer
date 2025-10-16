using AIChatServer.Entities.Connection.Interfaces;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Utils;
using AIChatServer.Utils.Interfaces;

namespace AIChatServer.Managers.Implementations
{
    public class TokenRefresher(ISerializer serializer, IConnectionTokenManager connectionTokenManager): ITokenRefresher
    {
        private readonly ISerializer _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        private readonly IConnectionTokenManager _connectionTokenManager = connectionTokenManager
            ?? throw new ArgumentNullException(nameof(connectionTokenManager));

        public async Task SendRefreshCommandAsync(IConnection connection, Guid userId)
        {
            var command = new CommandResponse("RefreshToken", new TokenResponse(_connectionTokenManager.GenerateToken(userId, connection.Id)));
            await CommandSender.SendCommandAsync(connection, command, _serializer);
        }
    }
}
