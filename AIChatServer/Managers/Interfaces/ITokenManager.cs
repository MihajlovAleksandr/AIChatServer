namespace AIChatServer.Managers.Interfaces
{
    public interface ITokenManager
    {
        string GenerateToken(Guid userId, Guid connectionId);
        (Guid userId, Guid connectionId, bool needToRefreshToken) ParseToken(string? token);
    }
}
