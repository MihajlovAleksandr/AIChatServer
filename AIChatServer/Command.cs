using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AIChatServer
{
    public class Command
    {
        [JsonInclude]
        private string operation;
        [JsonInclude]
        private Dictionary<string, object> data;
        [JsonIgnore]
        public string Operation { get { return operation; } }
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
    }
}
