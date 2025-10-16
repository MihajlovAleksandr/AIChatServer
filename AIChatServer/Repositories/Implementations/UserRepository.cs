using AIChatServer.Entities.User;
using AIChatServer.Repositories.Constants;
using AIChatServer.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;

namespace AIChatServer.Repositories.Implementations
{
    public class UserRepository(string connectionString, 
        ILogger<UserRepository> logger) : BaseRepository(connectionString), IUserRepository
    {
        private readonly ILogger<UserRepository> _logger = logger 
            ?? throw new ArgumentNullException(nameof(logger));

        public User GetUserById(Guid id)
        {

            try
            {
                _logger.LogInformation("Fetching user by ID {UserId}", id);

                using var connection = GetConnection();
                using var command = new NpgsqlCommand(UserQueries.GetUserById, connection);
                command.Parameters.AddWithValue("@userId", id);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                    return MapUserFromReader(reader);

                _logger.LogInformation("User with ID {UserId} not found.", id);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user by ID {UserId}.", id);
                throw;
            }
        }

        public User GetUserByEmail(string email)
        {

            try
            {
                _logger.LogInformation("Fetching user by email {Email}", email);

                using var connection = GetConnection();
                using var command = new NpgsqlCommand(UserQueries.GetUserByEmail, connection);
                command.Parameters.AddWithValue("@Email", email);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                    return MapUserFromReader(reader);

                _logger.LogInformation("User with email {Email} not found.", email);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user by email {Email}.", email);
                throw;
            }
        }

        public bool IsEmailFree(string email)
        {
            try
            {
                using var connection = GetConnection();
                using var command = new NpgsqlCommand(UserQueries.IsEmailFree, connection);
                command.Parameters.AddWithValue("@Email", email);

                long count = (long)command.ExecuteScalar();
                _logger.LogInformation("Email {Email} is free: {IsFree}", email, count == 0);
                return count == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if email {Email} is free.", email);
                throw;
            }
        }

        public Guid? AddUser(User user, Guid connectionId)
        {
            using var connection = GetConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                _logger.LogInformation("Adding new user with email {Email}", user.Email);

                Guid userId = InsertUser(user, connection, transaction);
                InsertUserData(user.UserData.Name, user.UserData.Gender, user.UserData.Age, userId, connection, transaction);
                InsertPreference(user.Preference.MinAge, user.Preference.MaxAge, user.Preference.Gender, userId, connection, transaction);
                UpdateConnection(connectionId, userId, connection, transaction);
                InsertNotifications(userId, connection, transaction);

                transaction.Commit();
                _logger.LogInformation("User {UserId} added successfully.", userId);

                return userId;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Error adding new user with email {Email}.", user.Email);
                return null;
            }
        }

        public bool UpdateUserData(string name, Gender gender, int age, Guid userId)
        {
            try
            {
                using var connection = GetConnection();
                using var command = new NpgsqlCommand(UserQueries.UpdateUserData , connection);
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Gender", gender.ToString());
                command.Parameters.AddWithValue("@Age", age);
                command.Parameters.AddWithValue("@UserId", userId);

                int affected = command.ExecuteNonQuery();
                _logger.LogInformation("Updated user data for {UserId}, affected rows: {Count}", userId, affected);

                return affected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user data for {UserId}.", userId);
                throw;
            }
        }

        public bool UpdatePreference(int minAge, int maxAge, PreferenceGender gender, Guid userId)
        {
            try
            {
                using var connection = GetConnection();
                using var command = new NpgsqlCommand(UserQueries.UpdatePreference, connection);
                command.Parameters.AddWithValue("@MinAge", minAge);
                command.Parameters.AddWithValue("@MaxAge", maxAge);
                command.Parameters.AddWithValue("@PreferredGender", gender.ToString());
                command.Parameters.AddWithValue("@UserId", userId);

                int affected = command.ExecuteNonQuery();
                _logger.LogInformation("Updated preference for {UserId}, affected rows: {Count}", userId, affected);

                return affected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating preference for {UserId}.", userId);
                throw;
            }
        }

        public bool IsUserPremium(Guid userId)
        {
            try
            {
                using var connection = GetConnection();
                using var command = new NpgsqlCommand(UserQueries.IsUserPremium, connection);
                command.Parameters.AddWithValue("@UserId", userId);

                long count = Convert.ToInt64(command.ExecuteScalar());
                _logger.LogInformation("User {UserId} premium status: {IsPremium}", userId, count > 0);

                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking premium status for {UserId}.", userId);
                throw;
            }
        }

        public Guid[] GetUsersInSameChats(Guid targetUserId)
        {
            var userIds = new List<Guid>();

            try
            {
                using var connection = GetConnection();
                using var command = new NpgsqlCommand(UserQueries.GetUsersInSameChats, connection);
                command.Parameters.AddWithValue("@targetUserId", targetUserId);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                    userIds.Add(reader.GetGuid(0));

                _logger.LogInformation("Fetched {Count} users in same chats as {UserId}", userIds.Count, targetUserId);

                return userIds.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users in same chats for {UserId}", targetUserId);
                throw;
            }
        }

        private User MapUserFromReader(NpgsqlDataReader reader)
        {
            return new User
            {
                Id = reader.GetGuid("id"),
                Email = reader.GetString("email"),
                Password = reader.IsDBNull("password") ? null : reader.GetString("password"),
                Premium = reader.IsDBNull("premium_until") ? null : reader.GetDateTime("premium_until"),
                GoogleId = reader.IsDBNull("google_id") ? null : reader.GetString("google_id"),
                UserData = new UserData
                {
                    Id = reader.GetGuid("user_data_id"),
                    Name = reader.GetString("name"),
                    Gender = (Gender)Enum.Parse(typeof(Gender), reader.GetString("gender")),
                    Age = reader.GetInt32("age")
                },
                Preference = new Preference
                {
                    Id = reader.GetGuid("preference_id"),
                    MinAge = reader.GetInt32("min_age"),
                    MaxAge = reader.GetInt32("max_age"),
                    Gender = (PreferenceGender)Enum.Parse(typeof(PreferenceGender), reader.GetString("preferred_gender"))
                }
            };
        }

        private Guid InsertUser(User user, NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            string query = user.GoogleId == null
                ? UserQueries.InsertUserWithPassword
                : UserQueries.InsertUserWithGoogle;

            using var command = new NpgsqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@Email", user.Email);

            if (user.GoogleId == null)
                command.Parameters.AddWithValue("@Password", HashPassword(user.Password));
            else
                command.Parameters.AddWithValue("@GoogleId", user.GoogleId);

            return (Guid)command.ExecuteScalar();
        }

        private void InsertUserData(string name, Gender gender, int age, Guid userId, NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            using var command = new NpgsqlCommand(UserQueries.InsertUserData, connection, transaction);
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@Name", name);
            command.Parameters.AddWithValue("@Gender", gender.ToString());
            command.Parameters.AddWithValue("@Age", age);
            command.ExecuteNonQuery();
        }

        private void InsertPreference(int minAge, int maxAge, PreferenceGender gender, Guid userId, NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            using var command = new NpgsqlCommand(UserQueries.InsertPreference, connection, transaction);
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@MinAge", minAge);
            command.Parameters.AddWithValue("@MaxAge", maxAge);
            command.Parameters.AddWithValue("@PreferredGender", gender.ToString());
            command.ExecuteNonQuery();
        }

        private void InsertNotifications(Guid userId, NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            using var command = new NpgsqlCommand(UserQueries.InsertNotifications, connection, transaction);
            command.Parameters.AddWithValue("@UserId", userId);
            command.ExecuteNonQuery();
        }

        private void UpdateConnection(Guid id, Guid userId, NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            using var command = new NpgsqlCommand(UserQueries.UpdateConnection, connection, transaction);
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@Id", id);
            command.ExecuteNonQuery();
        }

        private string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return string.Concat(bytes.Select(b => b.ToString("x2")));
        }
    }
}
