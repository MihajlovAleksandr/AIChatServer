using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Request
{
    public record MessageRequest(
        [property: JsonProperty("chat")] Guid Chat,
        [property: JsonProperty("sender")] Guid Sender,
        [property: JsonProperty("text")] string Text
    );
}
