using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Response
{
    public record DeleteConnectionResponse(
        [property: JsonProperty("connectionInfo")] ConnectionInfoResponse ConnectionInfo,
        [property: JsonProperty("count")] int[] Count
    );
}
