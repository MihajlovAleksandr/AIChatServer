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
        public int id;
        public int chat;
        public int sender;
        public string text;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? time;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (time == null)
            {
                time = DateTime.Now.ToUniversalTime();
            }
        }

        public override string ToString()
        {
            return $"Message {id}:\nFrom User{sender} To Chat{chat} In {JsonHelper.Serialize(time)}\n{text}";
        }
    }

}
