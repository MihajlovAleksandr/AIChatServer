using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Response
{
    public record UserOnlineChangesResponse(
        [property: JsonProperty("userId")] Guid UserId,
        [property: JsonProperty("isOnline")] bool IsOnline
    );
}
