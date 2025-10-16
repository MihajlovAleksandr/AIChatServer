using AIChatServer.Entities.Chats;
using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Request
{
    public record SearchChatRequest(
        [property: JsonProperty("chatType")] ChatType ChatType
    );
}
