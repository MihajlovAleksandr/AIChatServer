using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Response
{
    public record DeviceResponse(
        [property: JsonProperty("connectionInfo")] IReadOnlyCollection<ConnectionInfoResponse> ConnectionInfo,
        [property: JsonProperty("currentConnection")] Guid CurrentConnection
    );

}
