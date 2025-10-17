using AIChatServer.Entities.User.ServerUsers.Interfaces;
using Google.Apis.Auth;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Entities.User.ServerUsers.Implementations
{
    public class GoogleAuthValidator(string googleClientId, ILogger<GoogleAuthValidator> logger) : IOAuthValidator
    {
        private readonly string _googleClientId = googleClientId
            ?? throw new ArgumentNullException(nameof(googleClientId));
        private readonly ILogger<GoogleAuthValidator> _logger = logger 
            ?? throw new ArgumentNullException(nameof(logger));

        public async Task<OAuthUser?> Validate(string authToken)
        {
            if (string.IsNullOrWhiteSpace(authToken))
            {
                _logger.LogWarning("Received empty or null auth token.");
                return null;
            }

            GoogleJsonWebSignature.Payload? payload = await ValidateToken(authToken);

            if (payload == null)
            {
                _logger.LogWarning("Token validation failed: payload is null.");
                return null;
            }

            if ((string)payload.Audience != _googleClientId)
            {
                _logger.LogWarning("Token audience mismatch. Expected: {ClientId}, Actual: {Audience}", _googleClientId, payload.Audience);
                return null;
            }

            _logger.LogInformation("Token successfully validated for user {UserId} with email {Email}.", payload.Subject, payload.Email);
            return new OAuthUser(payload.Subject, payload.Email);
        }

        private async Task<GoogleJsonWebSignature.Payload?> ValidateToken(string idToken)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    ForceGoogleCertRefresh = true,
                    ExpirationTimeClockTolerance = TimeSpan.FromMinutes(5)
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                _logger.LogInformation("Google token successfully parsed for user {UserId}.", payload.Subject);
                return payload;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Google auth token.");
                return null;
            }
        }
    }
}
