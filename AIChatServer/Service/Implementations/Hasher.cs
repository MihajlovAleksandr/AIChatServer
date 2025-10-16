using AIChatServer.Service.Interfaces;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace AIChatServer.Service.Implementations
{
    public class Hasher(ILogger<Hasher> logger) : IHasher
    {
        private readonly ILogger<Hasher> _logger = logger 
            ?? throw new ArgumentNullException(nameof(logger));

        public string HashPassword(string password)
        {
            ArgumentNullException.ThrowIfNull(password);

            try
            {
                using (var sha256 = SHA256.Create())
                {
                    var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                    var builder = new StringBuilder();
                    foreach (byte b in bytes)
                    {
                        builder.Append(b.ToString("x2"));
                    }

                    string hash = builder.ToString();
                    _logger.LogInformation("Password hashed successfully. Hash length: {Length}", hash.Length);

                    return hash;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while hashing password.");
                throw;
            }
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            ArgumentNullException.ThrowIfNull(password);
            ArgumentNullException.ThrowIfNull(hashedPassword);

            try
            {
                string computedHash = HashPassword(password);
                bool isMatch = string.Equals(computedHash, hashedPassword, StringComparison.OrdinalIgnoreCase);

                if (isMatch)
                {
                    _logger.LogInformation("Password verification succeeded.");
                }
                else
                {
                    _logger.LogWarning("Password verification failed.");
                }

                return isMatch;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while verifying password.");
                throw;
            }
        }
    }
}
