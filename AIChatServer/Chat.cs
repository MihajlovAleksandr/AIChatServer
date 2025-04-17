using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AIChatServer
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
        public int[] Users { get; set; }
        public Chat()
        {
            CreationTime = DateTime.Now;
        }

        public int CompareTo(Chat other)
        {
            return CreationTime.CompareTo(other.CreationTime);
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
