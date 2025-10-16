using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Request
{
    public record DeleteConnectionRequest(
         [property: JsonProperty("connectionId")] Guid? ConnectionId
     );
}
