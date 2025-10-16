namespace AIChatServer.Entities.User.ServerUsers
{
    public class OAuthUser(string id, string email)
    {
        public string Id { get; private set; } = id;
        public string Email { get; private set; } = email;
    }
}
