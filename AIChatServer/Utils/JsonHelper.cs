using System.Text;
using Newtonsoft.Json;
namespace AIChatServer.Utils
{
    public static class JsonHelper
    {
        public static T Deserialize<T>(byte[] bytes)
        {
            var settings = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Populate
            };
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes), settings);
            //string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
        }
        public static T Deserialize<T>(string str)
        {
            var settings = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Populate
            };


            return JsonConvert.DeserializeObject<T>(str, settings);
            //string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
        }
        public static byte[] SerializeToBytes<T>(T obj)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj));
        }
        public static string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }


    }
}
