using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace AIChatServer
{
    public static class CommandConverter
    {
        public static Command GetCommand(byte[] bytes)
        {
            return JsonConvert.DeserializeObject<Command>(Encoding.UTF8.GetString(bytes));
            //string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
        }
        public static byte[] ParseCommand(Command command)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(command));
        }
    }
}
