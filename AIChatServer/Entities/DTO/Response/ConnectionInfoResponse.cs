using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Response
{
    public record ConnectionInfoResponse(
        [property: JsonProperty("id")] Guid Id,
        [property: JsonProperty("userId")] Guid UserId,
        [property: JsonProperty("device")] string Device,
        [property: JsonProperty("lastOnline")] DateTime? LastOnline);
}