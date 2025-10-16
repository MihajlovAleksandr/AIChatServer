using AIChatServer.Entities.User;
using AIChatServer.Repositories.Interfaces;
using AIChatServer.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Service.Implementations
{
    public class AuthService(IAuthRepository authRepository, IHasher hasher, ILogger<AuthService> logger) : IAuthService
    {
        private readonly IAuthRepository _authRepository = authRepository 
            ?? throw new ArgumentNullException(nameof(authRepository));
        private readonly IHasher _hasher = hasher 
            ?? throw new ArgumentNullException(nameof(hasher));
        private readonly ILogger<AuthService> _logger = logger 
            ?? throw new ArgumentNullException(nameof(logger));

        public User? Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("Login failed: email or password is empty");
                throw new ArgumentException("Email and password are required");
            }

            _logger.LogInformation("Attempting login for {Email}", email);
            
            var user = _authRepository.Login(email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: user not found for {Email}", email);
                return null;
            }

            if (VerifyPassword(user.Password, password))
            {
                _logger.LogInformation("Login successful for {Email}", email);
                return user;
            }

            _logger.LogWarning("Login failed: incorrect password for {Email}", email);
            return null;
        }

        public bool VerifyGoogleLogin(string email, string googleId)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(googleId))
            {
                _logger.LogWarning("Google login failed: email or Google ID is empty");
                throw new ArgumentException("Email and Google ID are required");
            }

            _logger.LogInformation("Verifying Google login for {Email}", email);
            return _authRepository.VerifyGoogleId(email, googleId);
        }

        public bool VerifyPassword(string? hashedPassword, string password)
        {
            if (hashedPassword == null)
            {
                _logger.LogWarning("Password verification failed: stored hash is null");
                return false;
            }

            bool isValid = _hasher.VerifyPassword(password, hashedPassword);
            if (!isValid)
            {
                _logger.LogWarning("Password verification failed");
            }

            return isValid;
        }

        public string ChangePassword(Guid userId, string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword))
            {
                _logger.LogWarning("Password change failed: new password is empty for UserId={UserId}", userId);
                throw new ArgumentException("New password is required");
            }

            string hashedPassword = _hasher.HashPassword(newPassword);
            _logger.LogInformation("Password changed for UserId={UserId}", userId);

            return _authRepository.ChangePassword(userId, hashedPassword);
        }

        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("Password hashing failed: password is empty");
                throw new ArgumentException("Password is required");
            }

            return _hasher.HashPassword(password);
        }
    }
}
