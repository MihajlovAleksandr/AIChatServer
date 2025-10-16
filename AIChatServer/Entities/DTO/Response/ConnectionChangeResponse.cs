using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Response
{
    public record ConnectionChangeResponse(
        [property: JsonProperty("connectionInfo")] ConnectionInfoResponse ConnectionInfo,
        [property: JsonProperty("count")] int[] Count,
        [property: JsonProperty("isOnline")] bool IsOnline
    );
}
