using AIChatServer.Entities.User;

namespace AIChatServer.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        bool VerifyGoogleId(string email, string googleId);
        User? Login(string emails);
        string ChangePassword(Guid userId, string password);
        UserBan? GetUserBanById(Guid userId);
    }
}