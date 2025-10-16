namespace AIChatServer.Utils.Interfaces
{
    public interface IConnectionTokenManager
    {
        string GenerateToken(Guid userId, Guid connectionId);
        bool ValidateToken(string token, out Guid userId, out Guid connectionId, out DateTime expirationTime);
    }
}
