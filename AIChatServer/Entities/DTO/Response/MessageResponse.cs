using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Response
{
    public record MessageResponse(
        [property: JsonProperty("id")] Guid Id,
        [property: JsonProperty("chat")] Guid Chat,
        [property: JsonProperty("sender")] Guid Sender,
        [property: JsonProperty("text")] string Text,
        [property: JsonProperty("time")] DateTime Time,
        [property: JsonProperty("lastUpdate")] DateTime LastUpdate

    );
}
