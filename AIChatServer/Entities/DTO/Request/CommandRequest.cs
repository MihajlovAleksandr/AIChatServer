using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Request
{
    public class CommandRequest
    {
        [JsonProperty("data")]
        public JObject? Data { get; }

        [JsonProperty("operation")]
        public string Operation { get; }

        [JsonConstructor]
        public CommandRequest(string operation, object? data = null)
        {
            Operation = operation;
            if (data == null)
                Data = null;
            else
                Data = JObject.FromObject(data);
        }

        public override string ToString()
        {
            return Data == null ? $"Command {{{Operation}}} With some data" : $"Command {{{Operation}}} Without data";
        }
    }
}

