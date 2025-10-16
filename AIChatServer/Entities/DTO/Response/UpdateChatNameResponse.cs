using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Response
{
    public record UpdateChatNameResponse(
       [property: JsonProperty("chatId")] Guid ChatId,
       [property: JsonProperty("name")] string Name
   );
}
