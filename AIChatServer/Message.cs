using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatServer
{
    using System;
    using Newtonsoft.Json;
    using System.Runtime.Serialization;

    public class Message
    {
        public int Id { get; set; }
        public int Chat { get; set; }
        public int Sender { get; set; }
        public string Text { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? Time { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime LastUpdate { get; set; }

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
