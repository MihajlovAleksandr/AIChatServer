namespace AIChatServer.Entities.AI
{
    public class AIMessage(Guid id, Guid chatId, string role, string content)
    {
        public Guid Id { get; set; } = id;
        public Guid ChatId { get; set; } = chatId;
        public string Role { get; set; } = role;
        public string Content { get; set; } = content;
    }
}