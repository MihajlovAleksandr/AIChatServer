namespace AIChatServer.Entities.Chats
{
    public class ChatWithUserContext(Chat chat, Guid userId)
    {
        public Chat Chat { get; } = chat;
        public Guid UserId { get; } = userId;
    }
}
