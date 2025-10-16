using AIChatServer.Entities.User;
using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Response
{
    public record PreferenceResponse(
         [property: JsonProperty("minAge")] int MinAge,
         [property: JsonProperty("maxAge")] int MaxAge,
         [property: JsonProperty("gender")] PreferenceGender Gender
     );
}
