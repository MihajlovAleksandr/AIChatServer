namespace AIChatServer.Entities.Chats
{
    public class Chat
    {
        public Guid Id { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? EndTime { get; set; }
        public ChatType Type { get; set; }
        public Dictionary<Guid, UserInChatData> UsersWithData { get; set; }

        public Chat()
        {
            UsersWithData = new Dictionary<Guid, UserInChatData>();
            CreationTime = DateTime.Now;
        }

        public bool ContainsAI(Guid aiId)
        {
            return UsersWithData.TryGetValue(aiId, out UserInChatData? _);
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Chat other) return false;
            return Id.Equals(other.Id);
        }

        public override string ToString()
        {
            return $"Chat #{Id}\n{CreationTime} - {EndTime}\n";
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
