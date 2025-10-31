using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Response
{
    public record UserAddingResponse
    (
        [property: JsonProperty("isUserAdding")] bool IsUserAdding,
        [property: JsonProperty("chatId")] Guid? ChatId
    );
}
