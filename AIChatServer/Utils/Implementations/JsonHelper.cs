using System.Text;
using AIChatServer.Utils.Interfaces;
using Newtonsoft.Json;

namespace AIChatServer.Utils.Implementations
{
    public class JsonHelper : ISerializer
    {
        public T Deserialize<T>(byte[] bytes)
        {
            var settings = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Populate
            };
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes), settings);
        }

        public T Deserialize<T>(string str)
        {
            var settings = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Populate
            };


            return JsonConvert.DeserializeObject<T>(str, settings);
        }

        public byte[] SerializeToBytes<T>(T obj)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj));
        }

        public string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
