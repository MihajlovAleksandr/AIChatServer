using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Request
{
    public record GoogleTokenRequest(
        [property: JsonProperty("token")] string Token
    )
    {
        public override string ToString() => "GoogleTokenRequest {}";
    }
}
