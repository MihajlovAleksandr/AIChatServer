using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Response
{
    public record ChatResponse(
        [property: JsonProperty("id")] Guid Id,
        [property: JsonProperty("name")] string Name,
        [property: JsonProperty("creationTime")] DateTime CreationTime,
        [property: JsonProperty("endTime")] DateTime? EndTime
    );
}
