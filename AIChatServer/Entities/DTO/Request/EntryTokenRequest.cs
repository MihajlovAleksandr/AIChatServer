using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Request
{
    public record EntryTokenRequest(
       [property: JsonProperty("token")] string Token
    )
    {
        public override string ToString() => "EntryTokenRequest {}";
    }
}
