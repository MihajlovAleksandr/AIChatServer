using Newtonsoft.Json;

namespace AIChatServer.Entities.Connection
{
    public class ConnectionInfo
    {
        [JsonProperty]
        private int id;
        [JsonProperty]
        private int userId;
        [JsonProperty]
        private string device;
        [JsonProperty]
        private DateTime? lastOnline;
        [JsonIgnore]
        public int Id { get { return id; } }
        [JsonIgnore]
        public int UserId { get { return userId; } }
        [JsonIgnore]
        public string Device { get { return device; } }
        [JsonIgnore]
        public DateTime? LastOnline { get { return lastOnline; } }
        public ConnectionInfo()
        {
            
        }
        public ConnectionInfo(int id, int userId, string device, DateTime? lastOnline)
        {
            this.id = id;
            this.userId = userId;
            this.device = device;
            this.lastOnline = lastOnline;
        }
        public override string ToString()
        {
            return $"ConnectionInfo {id}/{userId}:\n{device} in {lastOnline}";
        }
    }
}
