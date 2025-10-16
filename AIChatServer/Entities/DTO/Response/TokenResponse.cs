using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Response
{
    public record TokenResponse(
        [property: JsonProperty("token")] string Token
    )
    {
        public override string ToString() => "TokenResponse {}";
    }
}
