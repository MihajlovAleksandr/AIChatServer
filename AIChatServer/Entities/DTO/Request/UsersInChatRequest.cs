using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Request
{
    public record UsersInChatRequest(
        [property: JsonProperty("chatId")] Guid ChatId
    );
}
