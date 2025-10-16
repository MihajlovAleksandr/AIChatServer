using AIChatServer.Entities.User;
using AIChatServer.Repositories.Interfaces;
using AIChatServer.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Service.Implementations
{
    public class UserService(IUserRepository userRepository, ILogger<UserService> logger) : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository 
            ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly ILogger<UserService> _logger = logger 
            ?? throw new ArgumentNullException(nameof(logger));

        public User GetUserById(Guid id)
        {
            _logger.LogInformation("Fetching user by Id: {UserId}", id);
            try
            {
                var user = _userRepository.GetUserById(id);
                if (user == null)
                {
                    _logger.LogWarning("User with Id: {UserId} not found", id);
                }
                else
                {
                    _logger.LogInformation("Fetched user by Id: {UserId}", id);
                }
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching user by Id: {UserId}", id);
                throw;
            }
        }

        public User GetUserByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));

            _logger.LogInformation("Fetching user by Email: {Email}", email);
            try
            {
                var user = _userRepository.GetUserByEmail(email);
                if (user == null)
                {
                    _logger.LogWarning("User with Email: {Email} not found", email);
                }
                else
                {
                    _logger.LogInformation("Fetched user by Email: {Email}", email);
                }
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching user by Email: {Email}", email);
                throw;
            }
        }

        public bool IsEmailFree(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));

            _logger.LogInformation("Checking if email is free: {Email}", email);
            try
            {
                var result = _userRepository.IsEmailFree(email);
                _logger.LogInformation("Email: {Email} is free: {Result}", email, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while checking if email is free: {Email}", email);
                throw;
            }
        }

        public Guid? AddUser(User user, Guid connectionId)
        {
            ArgumentNullException.ThrowIfNull(user);

            _logger.LogInformation("Adding new user with ConnectionId: {ConnectionId}", connectionId);
            try
            {
                var userId = _userRepository.AddUser(user, connectionId);
                if (userId.HasValue)
                {
                    _logger.LogInformation("User added successfully. UserId: {UserId}, ConnectionId: {ConnectionId}", userId, connectionId);
                }
                else
                {
                    _logger.LogWarning("Failed to add user with ConnectionId: {ConnectionId}", connectionId);
                }
                return userId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding user with ConnectionId: {ConnectionId}", connectionId);
                throw;
            }
        }

        public bool UpdateUserData(UserData userData, Guid userId)
        {
            ArgumentNullException.ThrowIfNull(userData);

            _logger.LogInformation("Updating user data for UserId: {UserId}", userId);
            try
            {
                var result = _userRepository.UpdateUserData(userData.Name, userData.Gender, userData.Age, userId);
                _logger.LogInformation("User data update completed for UserId: {UserId}, Result: {Result}", userId, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating user data for UserId: {UserId}", userId);
                throw;
            }
        }

        public bool UpdatePreference(Preference preference, Guid userId)
        {
            ArgumentNullException.ThrowIfNull(preference);

            _logger.LogInformation("Updating preferences for UserId: {UserId}", userId);
            try
            {
                var result = _userRepository.UpdatePreference(preference.MinAge, preference.MaxAge, preference.Gender, userId);
                _logger.LogInformation("Preferences update completed for UserId: {UserId}, Result: {Result}", userId, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating preferences for UserId: {UserId}", userId);
                throw;
            }
        }

        public bool IsUserPremium(Guid userId)
        {
            _logger.LogInformation("Checking premium status for UserId: {UserId}", userId);
            try
            {
                var result = _userRepository.IsUserPremium(userId);
                _logger.LogInformation("Premium status for UserId: {UserId} = {Result}", userId, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while checking premium status for UserId: {UserId}", userId);
                throw;
            }
        }

        public Guid[] GetUsersInSameChats(Guid targetUserId)
        {
            _logger.LogInformation("Fetching users in the same chats as UserId: {UserId}", targetUserId);
            try
            {
                var users = _userRepository.GetUsersInSameChats(targetUserId);
                _logger.LogInformation("Found {Count} users in the same chats as UserId: {UserId}", users.Length, targetUserId);
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching users in the same chats as UserId: {UserId}", targetUserId);
                throw;
            }
        }
    }
}
