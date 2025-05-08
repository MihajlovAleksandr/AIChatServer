using Newtonsoft.Json;

namespace AIChatServer.Entities.Chats
{
    public class Chat : IComparable<Chat>
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("creationTime")]

        public DateTime CreationTime { get; set; }
        [JsonProperty("endTime")]

        public DateTime? EndTime { get; set; }
        [JsonIgnore]
        public string Type { get; set; }
        [JsonIgnore]
        public int[] Users { get; set; }
        public Chat()
        {
            CreationTime = DateTime.Now;
        }

        public int CompareTo(Chat other)
        {
            return CreationTime.CompareTo(other.CreationTime);
        }
        public bool ContainsAI(int aiId)
        {
            return Users.Contains(aiId);
        }
        public override bool Equals(object? obj)
        {
            Chat other = obj as Chat;
            if (other == null) return false;
            return Id.Equals(other.Id);
        }
        public override string ToString()
        {
            return $"Chat #{Id}\n{CreationTime} - {EndTime}\n";
        }
    }
}
