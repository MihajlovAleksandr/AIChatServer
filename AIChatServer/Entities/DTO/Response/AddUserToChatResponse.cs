using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Response
{
    public record AddUserToChatResponse
    (
        [property: JsonProperty("chatId")] Guid ChatId,
        [property: JsonProperty("userId")] Guid UserId,
        [property: JsonProperty("userData")] UserDataResponse UserData,
        [property: JsonProperty("isOnline")] bool IsOnline
    );
}
