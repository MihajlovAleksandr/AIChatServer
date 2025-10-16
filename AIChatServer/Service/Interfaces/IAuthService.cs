using AIChatServer.Entities.User;

namespace AIChatServer.Service.Interfaces
{
    public interface IAuthService
    {
        User? Login(string email, string password);
        bool VerifyGoogleLogin(string email, string googleId);
        bool VerifyPassword(string? hashedPassword, string password);
        string ChangePassword(Guid userId, string newPassword);
        string HashPassword(string password);
    }
}