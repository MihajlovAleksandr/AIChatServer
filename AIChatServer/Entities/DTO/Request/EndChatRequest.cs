using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Request
{
    public record EndChatRequest(
          [property: JsonProperty("chatId")] Guid ChatId
      );
}