using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Response
{
    public record UsersInChatResponse(
        [property: JsonProperty("ids")] IReadOnlyCollection<Guid> Ids,
        [property: JsonProperty("userData")] IReadOnlyCollection<UserDataResponse> UserData,
        [property: JsonProperty("isOnline")] IReadOnlyCollection<bool> IsOnline
    );
}
