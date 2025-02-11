using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.WebSockets;

namespace AIChatServer
{
    public class Command
    {
        [JsonProperty]
        private string operation;
        [JsonProperty]
        private Dictionary<string, object> data;
        [JsonIgnore]
        public string Operation { get { return operation; } }
        [JsonIgnore]
        public WebSocket Sender { get; private set; }
        public Command(string operation)
        {
            this.operation = operation;
            data = new Dictionary<string, object>();
        }
        public Command()
        {
        }
        public void AddData(string name, object obj)
        {
            data.Add(name, obj);
        }
        public T GetData<T>(string name)
        {
            if (data.TryGetValue(name, out var val)) return (T)val;
            return default;
        }
        public void SetSender(WebSocket sender)
        {
            Sender = sender;
        }
    }
}
