namespace AIChatServer.Service.Interfaces
{
    public interface IHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }
}
