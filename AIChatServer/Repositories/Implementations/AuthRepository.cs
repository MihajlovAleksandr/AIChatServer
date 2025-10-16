using AIChatServer.Entities.User;
using AIChatServer.Entities.Exceptions;
using AIChatServer.Repositories.Constants;
using AIChatServer.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AIChatServer.Repositories.Implementations
{
    public class AuthRepository(IUserRepository userRepository, string connectionString,
        ILogger<AuthRepository> logger) : BaseRepository(connectionString), IAuthRepository
    {
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly ILogger<AuthRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public bool VerifyGoogleId(string email, string googleId)
        {
            try
            {
                _logger.LogInformation("Verifying GoogleId for Email={Email}", email);

                using var connection = GetConnection();
                using var command = new NpgsqlCommand(AuthQueries.VerifyGoogleId, connection);
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@GoogleId", googleId);

                bool exists = Convert.ToInt64(command.ExecuteScalar()) > 0;

                if (exists)
                    _logger.LogInformation("GoogleId verification successful for Email={Email}", email);
                else
                    _logger.LogWarning("GoogleId verification failed for Email={Email}", email);

                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying GoogleId for Email={Email}", email);
                throw;
            }
        }

        public User? Login(string email)
        {
            try
            {
                _logger.LogInformation("Attempting login for Email={Email}", email);

                var ban = GetUserBanByEmail(email);
                if (ban != null)
                {
                    _logger.LogWarning("Login attempt blocked. User with Email={Email} is banned until {Until}", email, ban.BannedUntil);
                    throw new UserBannedException(ban);
                }

                var user = _userRepository.GetUserByEmail(email);

                if (user != null && user.Password != null)
                {
                    _logger.LogInformation("Login successful for Email={Email}", email);
                    return user;
                }

                _logger.LogWarning("Login failed. User not found or password is null for Email={Email}", email);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for Email={Email}", email);
                throw;
            }
        }

        public string ChangePassword(Guid userId, string hashedPassword)
        {
            try
            {
                _logger.LogInformation("Changing password for UserId={UserId}", userId);

                using var connection = GetConnection();
                using var command = new NpgsqlCommand(AuthQueries.ChangePassword, connection);
                command.Parameters.AddWithValue("@Password", hashedPassword);
                command.Parameters.AddWithValue("@Id", userId);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    _logger.LogInformation("Password successfully changed for UserId={UserId}", userId);
                    return hashedPassword;
                }

                _logger.LogWarning("Password change failed. UserId={UserId} not found", userId);
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for UserId={UserId}", userId);
                throw;
            }
        }

        private UserBan? GetUserBanByEmail(string email)
        {
            try
            {
                using var connection = GetConnection();
                using var command = new NpgsqlCommand(AuthQueries.GetUserBanByEmail, connection);
                command.Parameters.AddWithValue("@Email", email);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var ban = new UserBan(
                        id: reader.GetGuid(reader.GetOrdinal("id")),
                        userId: reader.GetGuid(reader.GetOrdinal("user_id")),
                        reason: reader.GetString(reader.GetOrdinal("reason")),
                        reasonCategory: Enum.Parse<BanReason>(reader.GetString(reader.GetOrdinal("reason_category"))),
                        bannedAt: reader.GetDateTime(reader.GetOrdinal("banned_at")),
                        bannedUntil: reader.GetDateTime(reader.GetOrdinal("banned_until"))
                    );

                    _logger.LogInformation("User with Email={Email} is banned until {Until}", email, ban.BannedUntil);
                    return ban;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ban info for Email={Email}", email);
                throw;
            }
        }

        public UserBan? GetUserBanById(Guid id)
        {
            try
            {
                using var connection = GetConnection();
                using var command = new NpgsqlCommand(AuthQueries.GetUserBanById, connection);
                command.Parameters.AddWithValue("@Id", id);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var ban = new UserBan(
                        id: reader.GetGuid(reader.GetOrdinal("id")),
                        userId: reader.GetGuid(reader.GetOrdinal("user_id")),
                        reason: reader.GetString(reader.GetOrdinal("reason")),
                        reasonCategory: Enum.Parse<BanReason>(reader.GetString(reader.GetOrdinal("reason_category"))),
                        bannedAt: reader.GetDateTime(reader.GetOrdinal("banned_at")),
                        bannedUntil: reader.GetDateTime(reader.GetOrdinal("banned_until"))
                    );

                    _logger.LogInformation("User with Email={Id} is banned until {Until}", id, ban.BannedUntil);
                    return ban;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ban info for Email={Id}", id);
                throw;
            }
        }
    }
}
