using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace AIChatServer.Entities.User
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PreferenceGender
    {
        Male,
        Female,
        Any
    }
}
