using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatServer.Entities.Chats
{
    public class ClientChat : IComparable<ClientChat>
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("creationTime")]

        public DateTime CreationTime { get; set; }
        [JsonProperty("endTime")]

        public DateTime? EndTime { get; set; }
        public ClientChat(Chat chat, int userId)
        {
            Id = chat.Id;
            if (!chat.UsersNames.TryGetValue(userId, out string? name)) throw new ArgumentException("userId must be in chat");
            Name = name;
            CreationTime = chat.CreationTime;
            EndTime = chat.EndTime;
        }
        public ClientChat(Chat chat, string name)
        {
            Id = chat.Id;
            Name = name;
            CreationTime = chat.CreationTime;
            EndTime = chat.EndTime;
        }
        public ClientChat(Chat chat)
        {
            Id = chat.Id;
            Name = "";
            CreationTime = chat.CreationTime;
            EndTime = chat.EndTime;
        }
        public ClientChat(int id, string name, DateTime creationTime, DateTime? endTime)
        {
            Id = id;
            Name = name;
            CreationTime = creationTime;
            EndTime = endTime;
        }

        public int CompareTo(ClientChat other)
        {
            return CreationTime.CompareTo(other.CreationTime);
        }
        public override bool Equals(object? obj)
        {
            ClientChat other = obj as ClientChat;
            if (other == null) return false;
            return Id.Equals(other.Id);
        }
        public override string ToString()
        {
            return $"Chat #{Id}: {Name}\n{CreationTime} - {EndTime}\n";
        }
    }
}
