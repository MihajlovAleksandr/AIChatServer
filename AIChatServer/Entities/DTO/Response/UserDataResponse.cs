using AIChatServer.Entities.User;
using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Response
{
    public record UserDataResponse(
       [property: JsonProperty("name")] string Name,
       [property: JsonProperty("age")] int Age,
       [property: JsonProperty("gender")] Gender Gender
   );
}
