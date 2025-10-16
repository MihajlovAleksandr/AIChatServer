using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Request
{
    public record UpdateChatNameRequest(
        [property: JsonProperty("chatId")] Guid ChatId,
        [property: JsonProperty("name")] string Name
    );
}
