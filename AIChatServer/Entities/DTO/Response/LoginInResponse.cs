using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Response
{
    public record LoginInResponse(
            [property: JsonProperty("userId")] Guid UserId
        );

}
