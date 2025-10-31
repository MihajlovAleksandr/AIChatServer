using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Response
{
    public record RemoveUserFromChatResponse
    (
        [property: JsonProperty("userId")] Guid UserId,
        [property: JsonProperty("chatId")] Guid ChatId
    );
}
