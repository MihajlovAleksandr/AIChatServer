using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Request
{
    public record RemoveUserFromChatRequest
    (
        [property: JsonProperty("userId")] Guid UserId,
        [property: JsonProperty("chatId")] Guid ChatId
    );
}
