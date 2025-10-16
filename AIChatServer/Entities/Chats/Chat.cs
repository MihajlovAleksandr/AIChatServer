namespace AIChatServer.Entities.Chats
{
    public class Chat
    {
        public Guid Id { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? EndTime { get; set; }
        public ChatType Type { get; set; }
        public Dictionary<Guid, string> UsersNames { get; set; }

        public Chat()
        {
            UsersNames = new Dictionary<Guid, string>();
            CreationTime = DateTime.Now;
        }

        public bool ContainsAI(Guid aiId)
        {
            return UsersNames.TryGetValue(aiId, out string? _);
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
