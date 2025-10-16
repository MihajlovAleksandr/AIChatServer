using AIChatServer.Entities.User;
using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Request
{
    public record PreferenceRequest(
        [property: JsonProperty("minAge")] int MinAge,
        [property: JsonProperty("maxAge")] int MaxAge,
        [property: JsonProperty("gender")] PreferenceGender Gender
    );
}
