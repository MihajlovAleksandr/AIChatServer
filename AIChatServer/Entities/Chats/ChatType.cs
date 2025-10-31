using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AIChatServer.Entities.Chats
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChatType
    {
        AI,
        Human,
        Random,
        Group
    }
}
