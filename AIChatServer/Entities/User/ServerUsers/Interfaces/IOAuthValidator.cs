namespace AIChatServer.Entities.User.ServerUsers.Interfaces
{
    public interface IOAuthValidator
    {
        Task<OAuthUser?> Validate(string authToken);
    }
}
