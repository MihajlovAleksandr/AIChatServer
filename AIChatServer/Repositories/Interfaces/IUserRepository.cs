using AIChatServer.Entities.User;

namespace AIChatServer.Repositories.Interfaces
{
    public interface IUserRepository
    {
        User GetUserById(Guid id);
        User GetUserByEmail(string email);
        bool IsEmailFree(string email);
        Guid? AddUser(User user, Guid connectionId);
        bool UpdateUserData(string name, Gender gender, int age, Guid userId);
        bool UpdatePreference(int minAge, int maxAge, PreferenceGender gender, Guid userId);
        bool IsUserPremium(Guid userId);
        Guid[] GetUsersInSameChats(Guid targetUserId);
    }
}