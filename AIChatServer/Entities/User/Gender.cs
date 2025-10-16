using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AIChatServer.Entities.User
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Gender
    {
        Male,
        Female,
        None
    }

}
