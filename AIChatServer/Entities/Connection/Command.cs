using Newtonsoft.Json;
using AIChatServer.Utils;

namespace AIChatServer.Entities.Connection
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
        public Connection Sender { get; private set; }
        public Command(string operation)
        {
            this.operation = operation;
            data = new Dictionary<string, string>();
        }
        public Command()
        {
        }
        public void AddData<T>(string name, T? obj)
        {
            data.Add(name, JsonHelper.Serialize(obj));
        }
        public T GetData<T>(string name)
        {

            if (data.TryGetValue(name, out var val))
            {
                Console.WriteLine(val);
                return JsonHelper.Deserialize<T>(val); 
            }

            Console.WriteLine("null");
            return default;
        }
        public void SetSender(Connection sender)
        {
            Sender = sender;
        }
        public override string ToString()
        {
            string info = $"{operation}: \nData count: {data.Count}:\n";
            foreach (var obj in data)
            {
                info += obj.ToString()+ "\n";
            }
            return  info;
        }
    }
}
