using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Response
{
    public record SearchChatResponse(
        [property: JsonProperty("isChatSearching")] bool IsChatSearching
    );
}
