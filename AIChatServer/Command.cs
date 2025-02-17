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
        private Dictionary<string,string> data;
        [JsonIgnore]
        public string Operation { get { return operation; } }
        [JsonIgnore]
        public WebSocket Sender { get; private set; }
        public Command(string operation)
        {
            this.operation = operation;
            data = new Dictionary<string, string>();
        }
        public Command()
        {
        }
        public void AddData<T>(string name, T obj)
        {
            data.Add(name, JsonHelper.Serialize(obj));
        }
        public T GetData<T>(string name)
        {

            if (data.TryGetValue(name, out var val))
            {
                Console.WriteLine(val);
                return (T)JsonHelper.Deserialize<T>(val); 
            }

            Console.WriteLine("null");
            return default;
        }
        public void SetSender(WebSocket sender)
        {
            Sender = sender;
        }
        public override string ToString()
        {
            return $"{operation}: \nData count: {data.Count}";
        }
    }
}
