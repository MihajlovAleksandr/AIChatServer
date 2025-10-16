namespace AIChatServer.Utils.Interfaces
{
    public interface IEntryTokenManager
    {
        string GenerateEntryToken(Guid userId);
        bool ValidateEntryToken(string token, out Guid userId);
    }
}
