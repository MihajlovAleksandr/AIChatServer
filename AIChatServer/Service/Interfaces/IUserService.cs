using AIChatServer.Entities.User;

namespace AIChatServer.Service.Interfaces
{
    public interface IUserService
    {
        User GetUserById(Guid id);
        User GetUserByEmail(string email);
        bool IsEmailFree(string email);
        Guid? AddUser(User user, Guid connectionId);
        bool UpdateUserData(UserData userData, Guid userId);
        bool UpdatePreference(Preference preference, Guid userId);
        bool IsUserPremium(Guid userId);
        Guid[] GetUsersInSameChats(Guid targetUserId);
    }
}