using AIChatServer.Entities.Chats;
using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Request
{
    public record AddUserToChatRequest
    (
        [property: JsonProperty("chatMatchPredicate")] string? ChatMatchPredicate,
        [property: JsonProperty("chatType")] ChatType ChatType
    );
}