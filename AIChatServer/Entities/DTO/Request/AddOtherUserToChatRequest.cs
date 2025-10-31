using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Request
{
    public record AddOtherUserToChatRequest
    (
        [property: JsonProperty("chatMatchPredicate")] string? ChatMatchPredicate,
        [property: JsonProperty("chatId")] Guid ChatId
    );
}
