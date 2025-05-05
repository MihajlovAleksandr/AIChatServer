using Newtonsoft.Json;
using System.Runtime.Serialization;
using AIChatServer.Utils;

namespace AIChatServer.Entities.Chats
{
    public class Message
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("chat")]

        public int Chat { get; set; }
        [JsonProperty("sender")]

        public int Sender { get; set; }
        [JsonProperty("text")]

        public string Text { get; set; }

        [JsonProperty("time", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? Time { get; set; }

        [JsonProperty("lastUpdate", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? LastUpdate { get; set; }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (Time == null)
            {
                Time = DateTime.Now.ToUniversalTime();
            }
            if (LastUpdate == null)
            {
                LastUpdate = DateTime.Now.ToUniversalTime();
            }

        }

        public override string ToString()
        {
            return $"Message {Id}:\nFrom User{Sender} To Chat{Chat} In {JsonHelper.Serialize(Time)}\nUpdate in {JsonHelper.Serialize(LastUpdate)}\n{Text}";
        }
    }

}
