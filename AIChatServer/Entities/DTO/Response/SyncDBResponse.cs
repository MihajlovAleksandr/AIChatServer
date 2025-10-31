using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Response
{
    public record SyncDBResponse(
        [property: JsonProperty("newMessages")] IReadOnlyCollection<MessageResponse> NewMessages,
        [property: JsonProperty("oldMessages")] IReadOnlyCollection<MessageResponse> OldMessages,
        [property: JsonProperty("newChats")] IReadOnlyCollection<ChatResponse> NewChats,
        [property: JsonProperty("oldChats")] IReadOnlyCollection<ChatResponse> OldChats,
        [property: JsonProperty("isChatSearching")] bool IsChatSearching,
        [property: JsonProperty("userAddingToChat")] Guid? UserAddingToChat
    );
}
